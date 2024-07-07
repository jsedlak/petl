using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Petl.EventSourcing.Providers;

public class MongoDbEventLog<TView, TEvent> : IEventLog<TView, TEvent>
    where TView : class, new()
    where TEvent : class
{
    private readonly MongoDbEventLogSettings _settings;
    private readonly IMongoClient _client;
    private readonly IEventSerializer _eventSerializer;
    private readonly ILogger<MongoDbEventLog<TView, TEvent>> _logger;

    private IEnumerable<EventLogEntry<byte[]>> _events = [];
    private readonly Queue<EventLogEntry<byte[]>> _eventQueue = new();

    private readonly TView _state = new();
    private readonly TView _tentativeState = new();

    private int _tentativeVersion = 0;
    private int _confirmedVersion = 0;
    
    public MongoDbEventLog(
        MongoDbEventLogSettings settings, 
        IMongoClient client,
        IEventSerializer eventSerializer,
        ILogger<MongoDbEventLog<TView, TEvent>> logger)
    {
        _settings = settings;
        _client = client;
        _eventSerializer = eventSerializer;
        _logger = logger;
    }
    
    private void ApplyTentative(TEvent @event)
    {
        dynamic e = @event;
        dynamic s = _tentativeState;
        s.Apply(e);
    }
    
    private void ApplyConfirmed(TEvent @event)
    {
        dynamic e = @event;
        dynamic s = _state;
        s.Apply(e);
    }

    private async Task SaveQueueToStorage()
    {
        var db = _client.GetDatabase(EventDatabaseName);
        var col = db.GetCollection<EventLogEntry<byte[]>>(_settings.GrainType);
        
        while (_eventQueue.Count > 0)
        {
            var eventRecord = _eventQueue.Dequeue();
            
            await col.InsertOneAsync(eventRecord);

            // apply  to the confirmed version
            var @event = _eventSerializer.Deserialize<TEvent>(eventRecord.Data);
            ApplyConfirmed(@event);
            
            // this is fishy....
            _confirmedVersion = Math.Max(eventRecord.Version, _confirmedVersion);
        }
    }
    
    public Task Hydrate()
    {
        var db = _client.GetDatabase(EventDatabaseName);
        var col = db.GetCollection<EventLogEntry<byte[]>>(_settings.GrainType);
        
        // TODO: Look for a snapshot. If it exists, we load that directly
        // TODO: If there is a snapshot, we need to do a delta from snapshot version to log tail

        _events = col.AsQueryable()
            .Where(m => m.GrainId == _settings.GrainId)
            .OrderBy(m => m.Version);
        
        // loop through the events to hydrate the state
        // these should be in a sequence ordered by version...
        foreach (var ev in _events)
        {
            TEvent @event = _eventSerializer.Deserialize<TEvent>(ev.Data);
            ApplyTentative(@event);
            ApplyConfirmed(@event);
            _tentativeVersion = _confirmedVersion = ev.Version;
        }

        return Task.CompletedTask;
    }

    public void Submit(TEvent entry)
    {
        ApplyTentative(@entry);
        
        _eventQueue.Enqueue(
            new EventLogEntry<byte[]>(
                Guid.NewGuid(), 
                _settings.GrainId,
                _eventSerializer.Serialize(entry).ToArray(), 
                ++_tentativeVersion
            )
        );
    }

    public void Submit(IEnumerable<TEvent> entries)
    {
        foreach (var @event in entries)
        {
            Submit(@event);
        }
    }

    public Task Snapshot(bool truncate)
    {
        return Task.CompletedTask;
    }

    public Task WaitForConfirmation()
    {
        return SaveQueueToStorage();
    }

    public TView TentativeView => _tentativeState;

    public TView ConfirmedView => _state;
    
    public int ConfirmedVersion => _confirmedVersion;

    public int TentativeVersion => _tentativeVersion;

    private string EventDatabaseName => $"{_settings.DatabaseName}-events";

    private string SnapshotDatabaseName => $"{_settings.DatabaseName}-snapshots";
}
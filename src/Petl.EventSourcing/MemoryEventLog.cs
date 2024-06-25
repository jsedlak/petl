namespace Petl.EventSourcing;

public sealed class MemoryEventLog<TView, TEntry> : IEventLog<TView, TEntry>
    where TView : class, new()
    where TEntry : class
{
    private readonly TView _state = new();
    private int _version = 0;
    
    public Task Hydrate()
    {
        // We cannot hydrate in a memory event log as there is no persistance
        return Task.CompletedTask;
    }
    
    private void Apply(TEntry @event)
    {
        dynamic e = @event;
        dynamic s = _state;
        s.Apply(e);
    }

    public void Submit(TEntry @event)
    {
        Apply(@event);
        _version++;
    }

    public void Submit(IEnumerable<TEntry> events)
    {
        foreach (var ev in events)
        {
            Submit(ev);
        }
    }

    public Task Snapshot(bool truncate)
    {
        // State is always up-to-date in memory model and event log is not persisted
        return Task.CompletedTask;
    }

    public Task WaitForConfirmation()
    {
        // There is no waiting for confirmation, memory writes are instant
        return Task.CompletedTask;
    }

    public TView TentativeView => _state;

    public TView ConfirmedView => _state;

    public int ConfirmedVersion => _version;

    public int TentativeVersion => _version;
}
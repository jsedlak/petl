using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;

namespace Petl.EventSourcing;

public abstract class EventSourcedGrain<TGrainState, TEventBase> : Grain, ILifecycleParticipant<IGrainLifecycle>
    where TGrainState : class, new()
    where TEventBase : class
{
    private IDisposable? _saveTimer;
    private ISnapshotStrategy<TGrainState>? _snapshotStrategy;
    private IEventLog<TGrainState, TEventBase> _eventLog = null!;
    
    public virtual void Participate(IGrainLifecycle lifecycle)
    {
        lifecycle.Subscribe<EventSourcedGrain<TGrainState, TEventBase>>(
            GrainLifecycleStage.SetupState,
            OnSetup,
            OnTearDown
        );

        lifecycle.Subscribe<EventSourcedGrain<TGrainState, TEventBase>>(
            GrainLifecycleStage.Activate - 1,
            OnHydrateState,
            OnDestroyState
        );
    }
    
    #region Interaction
    protected virtual void Raise(TEventBase @event)
    {
        _eventLog.Submit(@event);
    }

    protected virtual void Raise(IEnumerable<TEventBase> events)
    {
        _eventLog.Submit(events);
    }

    protected Task WaitForConfirmation()
    {
        return _eventLog.WaitForConfirmation();
    }

    protected Task Snapshot(bool truncate)
    {
        return _eventLog.Snapshot(truncate);
    }
    
    protected virtual Task OnSaveTimerTicked(object arg)
    {
        return Task.CompletedTask;
    }
    #endregion

    #region Service Setup / TearDown
    /// <summary>
    /// Disposes of any references
    /// </summary>
    private Task OnTearDown(CancellationToken token) => Task.CompletedTask;

    /// <summary>
    /// Grabs all required services from the ServiceProvider
    /// </summary>
    private Task OnSetup(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        
        // grab the event log factory and build the log service
        var factory = ServiceProvider.GetRequiredService<IEventLogFactory>();
        _eventLog = factory.Create<TGrainState, TEventBase>(GetType(), this.GetGrainId().ToString());

        // attempt to grab a snapshot strategy
        var snapshotFactory = ServiceProvider.GetService<ISnapshotStrategyFactory>();
        if (snapshotFactory != null)
        {
            _snapshotStrategy = snapshotFactory.Create<TGrainState>(GetType(), this.GetGrainId().ToString());
        }
        
        return Task.CompletedTask;
    }
    #endregion

    #region Hydrate / Destroy
    /// <summary>
    /// Responsible for hydrating the state from storage
    /// </summary>
    private async Task OnHydrateState(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }

        await _eventLog.Hydrate();

        var timer = GetType().GetCustomAttribute<PersistTimerAttribute>()?.Time ??
                        PersistTimerAttribute.DefaultTime;

        _saveTimer = RegisterTimer(OnSaveTimerTicked, null, timer, timer);
    }

    /// <summary>
    /// Called when the grain is deactivating
    /// </summary>
    private async Task OnDestroyState(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }

        await _eventLog.WaitForConfirmation();

        if (_saveTimer is not null)
        {
            _saveTimer.Dispose();
            _saveTimer = null;
        }
    }
    #endregion

    protected TGrainState State => _eventLog.ConfirmedView;

    protected int Version => _eventLog.ConfirmedVersion;
}
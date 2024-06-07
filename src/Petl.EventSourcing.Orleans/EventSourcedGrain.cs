using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace Petl.EventSourcing;

public abstract class EventSourcedGrain<TGrainState, TEventBase> : Grain, ILifecycleParticipant<IGrainLifecycle>
    where TGrainState : class, new()
    where TEventBase : class
{
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
        
        var factory = ServiceProvider.GetRequiredService<IEventLogFactory>();
        _eventLog = factory.Create<TGrainState, TEventBase>(GetType(), this.GetGrainId().ToString());
        
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
    }

    /// <summary>
    /// Called when the grain is deactivating
    /// </summary>
    private Task OnDestroyState(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        return _eventLog.WaitForConfirmation();
    }
    #endregion

    protected TGrainState State => _eventLog.ConfirmedView;

    protected int Version => _eventLog.ConfirmedVersion;
}
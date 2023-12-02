namespace Petl;

public interface IEventHandler<TEvent> 
{
    Task HandleAsync(EventContext context, TEvent @event, CancellationToken cancellationToken);
}

namespace Petl;

public interface IAsyncEventHandler<TEvent> 
{
    Task HandleAsync(EventContext context, TEvent @event, CancellationToken cancellationToken);
}

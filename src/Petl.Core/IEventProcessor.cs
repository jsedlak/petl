namespace Petl;

public interface IEventProcessor 
{
    Task ProcessAsync(IEvent @event, CancellationToken cancellationToken);
}
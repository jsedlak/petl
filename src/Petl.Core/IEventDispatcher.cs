namespace Petl;

public interface IEventDispatcher
{
    Task DispatchAsync(IEnumerable<IEvent> events);
}

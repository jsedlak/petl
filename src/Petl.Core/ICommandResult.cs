namespace Petl;

public interface ICommandResult 
{
    IEnumerable<IEvent> Events { get; }
}

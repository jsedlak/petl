namespace Petl;

/// <summary>
/// Responsible for processing commands, dispatching the resulting events.
/// </summary>
public interface ICommandProcessor
{
    Task<TResult> ProcessAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
        where TResult : ICommandResult;
}

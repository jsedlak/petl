namespace Petl;

public interface ICommandHandler<TCommand, TResult>
    where TResult : ICommandResult
{
    Task<TResult> ProcessAsync(CommandContext context, TCommand command, CancellationToken cancellationToken);
}

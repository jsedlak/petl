namespace Petl;

/// <summary>
/// Responsible for processing commands, dispatching the resulting events.
/// </summary>
public interface IRequestProcessor
{
    Task<TResult> ProcessAsync<TRequest, TResult>(TRequest command, CancellationToken cancellationToken)
        where TResult : IRequestResult;

    Task<TResult> ProcessAsync<TRequest, TResult>(RequestContext context, TRequest command, CancellationToken cancellationToken)
        where TResult : IRequestResult;
}

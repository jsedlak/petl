namespace Petl.Mediator;

public interface IRequestHandler<TRequest, TResponse>
    where TResponse : IRequestResult
{
    Task<TResponse> ProcessAsync(RequestContext context, TRequest command, CancellationToken cancellationToken);
}

namespace Petl.Mediator;

public interface IResponseDispatcher
{
    Task DispatchAsync(ResponseContext responseContext, IEnumerable<IResponse> responses, CancellationToken cancellationToken);
}

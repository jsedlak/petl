namespace Petl;

public interface IResponseHandler<TResponse> 
{
    Task HandleAsync(ResponseContext context, TResponse response, CancellationToken cancellationToken);
}

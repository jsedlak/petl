using Microsoft.Extensions.DependencyInjection;

namespace Petl;

public class RequestProcessor : IRequestProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public RequestProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> ProcessAsync<TRequest, TResult>(RequestContext context, TRequest command, CancellationToken cancellationToken)
        where TResult : IRequestResult
    {
        // create a service scope
        using var scope = _serviceProvider.CreateAsyncScope();

        // get the handler
        var handler = scope.ServiceProvider.GetService<IRequestHandler<TRequest, TResult>>();

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for request {typeof(TRequest).Name} and response {typeof(TResult).Name}");
        }

        var result = await handler.ProcessAsync(context, command, cancellationToken);

        // dispatch events if we have any
        if (result.Responses.Any())
        {
            // hydrate the response context from the original request context
            var responseContext = new ResponseContext
            {
                CorrelationId = context.CorrelationId,
                UserIdentfier = context.UserIdentifier
            };

            var dispatchers = scope.ServiceProvider.GetServices<IResponseDispatcher>();

            foreach (var dispatcher in dispatchers)
            {
                await dispatcher.DispatchAsync(responseContext, result.Responses, cancellationToken);
            }
        }

        return result;
    }

    public Task<TResult> ProcessAsync<TRequest, TResult>(TRequest command, CancellationToken cancellationToken) 
        where TResult : IRequestResult
    {
        // create the context
        var context = new RequestContext();

        return ProcessAsync<TRequest, TResult>(context, command, cancellationToken);
    }
}

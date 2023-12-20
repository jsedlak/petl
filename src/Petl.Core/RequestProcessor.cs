using Microsoft.Extensions.DependencyInjection;

namespace Petl;

public sealed class RequestProcessor : IRequestProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public RequestProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> ProcessAsync<TRequest, TResult>(TRequest command, CancellationToken cancellationToken) 
        where TResult : IRequestResult
    {
        // create the context
        var context = new RequestContext();

        // create a service scope
        using var scope = _serviceProvider.CreateAsyncScope();

        // get the handler
        var handler = scope.ServiceProvider.GetService<IRequestHandler<TRequest, TResult>>();

        if(handler == null)
        {
            throw new InvalidOperationException($"No handler found for request {typeof(TRequest).Name} and response {typeof(TResult).Name}");
        }

        var result = await handler.ProcessAsync(context, command, cancellationToken);

        // dispatch events if we have any
        if (result.Responses.Any())
        {
            var responseContext = new ResponseContext { CorrelationId = context.CorrelationId };

            var dispatchers = scope.ServiceProvider.GetServices<IResponseDispatcher>();

            foreach (var dispatcher in dispatchers)
            {
                await dispatcher.DispatchAsync(responseContext, result.Responses, cancellationToken);
            }
        }

        return result;
    }
}

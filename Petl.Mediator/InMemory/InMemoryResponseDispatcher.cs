using Microsoft.Extensions.DependencyInjection;

namespace Petl.Mediator.InMemory;

public class InMemoryResponseDispatcher : IResponseDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public InMemoryResponseDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(ResponseContext responseContext, IEnumerable<IResponse> responses, CancellationToken cancellationToken)
    {
        // create a service scope
        using var scope = _serviceProvider.CreateAsyncScope();

        // allow for pre-dispatch processing
        await PreDispatchAsync(responseContext, responses, scope.ServiceProvider, cancellationToken);

        foreach (var response in responses)
        {
            var wrapperType = typeof(HandlerImplementation<>).MakeGenericType(response.GetType());

            var wrapper = (HandlerWrapper)ActivatorUtilities.CreateInstance(scope.ServiceProvider, wrapperType);

            await wrapper.HandleAsync(responseContext, response, scope.ServiceProvider, async (handlers, theResponse, theToken) =>
            {
                foreach (var handler in handlers)
                {
                    // TODO: Do we need to pass in responseContext?
                    await handler.Callback(responseContext, theResponse, theToken);
                }
            }, cancellationToken);
        }
    }

    protected virtual Task PreDispatchAsync(ResponseContext responseContext, IEnumerable<IResponse> responses, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

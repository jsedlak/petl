using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Petl.Mediator.InMemory;

internal class HandlerImplementation<TResponse> : HandlerWrapper where TResponse : IResponse
{
    public override Task HandleAsync(ResponseContext responseContext, IResponse notification, IServiceProvider serviceProvider, Func<IEnumerable<HandlerExecutor>, IResponse, CancellationToken, Task> handle, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider
            .GetServices<IResponseHandler<TResponse>>()
            .Where(m => m.GetType().GetCustomAttribute<ResponseDispatcherAttribute>() == null || m.GetType().GetCustomAttribute<ResponseDispatcherAttribute>()?.DispatcherType == typeof(InMemoryResponseDispatcher))
            .Select(static x => new HandlerExecutor(
                x, 
                (theResponseContext, theResponse, theToken) => x.HandleAsync(theResponseContext, (TResponse)theResponse, theToken)
            ));

        return handle(handlers, notification, cancellationToken);
    }
}

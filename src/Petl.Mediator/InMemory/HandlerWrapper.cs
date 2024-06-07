namespace Petl.Mediator.InMemory;

internal abstract class HandlerWrapper
{
    public abstract Task HandleAsync(ResponseContext context, IResponse response, IServiceProvider serviceProvider, Func<IEnumerable<HandlerExecutor>, IResponse, CancellationToken, Task> handle, CancellationToken cancellationToken);
}

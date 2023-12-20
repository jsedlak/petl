namespace Petl.InMemory;

internal record HandlerExecutor(object Instance, Func<ResponseContext, IResponse, CancellationToken, Task> Callback);
namespace Petl.Tests;

public sealed class DummyResponseDispatcher : IResponseDispatcher
{
    public Task DispatchAsync(ResponseContext responseContext, IEnumerable<IResponse> responses, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

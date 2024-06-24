using Petl.Mediator;

namespace Petl.Tests.MediatorTests;

public sealed class DummyResponseDispatcher : IResponseDispatcher
{
    public Task DispatchAsync(ResponseContext responseContext, IEnumerable<IResponse> responses, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Petl.Mediator;
using Petl.Mediator.InMemory;

namespace Petl.Tests;

[ResponseDispatcher(typeof(InMemoryResponseDispatcher))]
public sealed class AttributedResponseHandler : IResponseHandler<CountIncreasedResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public AttributedResponseHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task HandleAsync(ResponseContext context, CountIncreasedResponse response, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Attributed Handler Invoked");

        _serviceProvider.GetRequiredService<TestTracker>().AttributedHandlerInvoked = true;

        return Task.CompletedTask;
    }
}

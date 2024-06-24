using Microsoft.Extensions.DependencyInjection;
using Petl.Mediator;

namespace Petl.Tests.MediatorTests;

[ResponseDispatcher(typeof(DummyResponseDispatcher))]
public sealed class IncorrectlyAttributedResponseHandler : IResponseHandler<CountIncreasedResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public IncorrectlyAttributedResponseHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task HandleAsync(ResponseContext context, CountIncreasedResponse response, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Incorrectly Attributed Handler Invoked");

        _serviceProvider.GetRequiredService<TestTracker>().IncorrectlyAttributedHandlerInvoked = true;

        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Petl.Mediator;

namespace Petl.Tests.MediatorTests;

public sealed class UnattributedResponseHandler : IResponseHandler<CountIncreasedResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public UnattributedResponseHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task HandleAsync(ResponseContext context, CountIncreasedResponse response, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Unattributed Handler Invoked");

        _serviceProvider.GetRequiredService<TestTracker>().UnattributedHandlerInvoked = true;

        return Task.CompletedTask;
    }
}

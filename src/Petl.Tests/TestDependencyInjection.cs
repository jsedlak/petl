using Microsoft.Extensions.DependencyInjection;
using Petl.InMemory;

namespace Petl.Tests;

[TestClass]
public class TestDependencyInjection
{
    [TestMethod]
    public async Task TestInMemoryDispatcher()
    {
        var testTracker = new TestTracker();
        var services = new ServiceCollection();
        
        services.AddSingleton<TestTracker>(sp => testTracker);

        services.AddPetl(builder => builder
            .WithInMemoryDispatcher()
            .RegisterRequestHandlers(typeof(TestRequestHandler).Assembly)
            .RegisterResponseHandlers(typeof(AttributedResponseHandler).Assembly)
        );

        var provider = services.BuildServiceProvider();

        var requestProcessor = provider.GetRequiredService<IRequestProcessor>();

        await requestProcessor.ProcessAsync<TestRequest, TestResult>(
            new TestRequest { Count = 10 }, 
            CancellationToken.None
        );

        var tracker = provider.GetRequiredService<TestTracker>();
        Assert.IsTrue(tracker.AttributedHandlerInvoked);
        Assert.IsTrue(tracker.UnattributedHandlerInvoked);
        Assert.IsFalse(tracker.IncorrectlyAttributedHandlerInvoked);
    }
}

public sealed class TestTracker
{
    public bool AttributedHandlerInvoked { get; set; }

    public bool UnattributedHandlerInvoked { get; set; }

    public bool IncorrectlyAttributedHandlerInvoked { get; set; }
}

using Microsoft.Extensions.DependencyInjection;
using Petl.Mediator;
using Petl.Mediator.InMemory;

namespace Petl.Tests.MediatorTests;

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
        Assert.IsTrue((bool)tracker.AttributedHandlerInvoked);
        Assert.IsTrue((bool)tracker.UnattributedHandlerInvoked);
        Assert.IsFalse((bool)tracker.IncorrectlyAttributedHandlerInvoked);
    }
}
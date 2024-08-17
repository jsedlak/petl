using Petl.Mediator;
using RequestContext = Petl.Mediator.RequestContext;

namespace Petl.Tests.MediatorTests;

public sealed class TestRequestHandler : IRequestHandler<TestRequest, TestResult>
{
    public Task<TestResult> ProcessAsync(RequestContext context, TestRequest command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TestResult
        {
            Responses = new[] {
                new CountIncreasedResponse{ OldCount = command.Count, NewCount = command.Count + 1 }
            }
        });
    }
}

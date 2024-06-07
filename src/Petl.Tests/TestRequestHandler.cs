using Petl.Mediator;

namespace Petl.Tests;

public sealed class TestRequestHandler : IRequestHandler<TestRequest, TestResult>
{
    public Task<TestResult> ProcessAsync(RequestContext context, TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TestResult
        {
            Responses = new[] {
                new CountIncreasedResponse{ OldCount = request.Count, NewCount = request.Count + 1 }
            }
        });
    }
}

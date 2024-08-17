using Petl.Mediator;

namespace Petl.Tests.MediatorTests;

public class TestResult : IRequestResult
{
    public IEnumerable<IResponse> Responses { get; set; } = Enumerable.Empty<IResponse>();
}

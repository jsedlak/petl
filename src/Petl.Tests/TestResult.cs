namespace Petl.Tests;

public class TestResult : IRequestResult
{
    public IEnumerable<IResponse> Responses { get; set; } = Enumerable.Empty<IResponse>();
}

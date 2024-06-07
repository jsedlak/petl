namespace Petl.Mediator;

public class RequestResult : IRequestResult
{
    public RequestResult()
    {

    }

    public RequestResult(IEnumerable<IResponse> responses)
    {
        Responses = responses;
    }

    public RequestResult(params IResponse[] responses)
    {
        Responses = responses;
    }

    public IEnumerable<IResponse> Responses { get; } = Enumerable.Empty<IResponse>();
}
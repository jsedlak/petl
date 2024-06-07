namespace Petl.Mediator;

public interface IRequestResult
{
    IEnumerable<IResponse> Responses { get; }
}

namespace Petl;

public interface IRequestResult
{
    IEnumerable<IResponse> Responses { get; }
}

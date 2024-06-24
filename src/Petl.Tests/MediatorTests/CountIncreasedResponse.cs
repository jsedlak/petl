using Petl.Mediator;

namespace Petl.Tests.MediatorTests;

public class CountIncreasedResponse : IResponse
{
    public int OldCount { get; set; }

    public int NewCount { get; set; }
}
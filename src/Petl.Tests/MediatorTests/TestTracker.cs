namespace Petl.Tests.MediatorTests;

public sealed class TestTracker
{
    public bool AttributedHandlerInvoked { get; set; }

    public bool UnattributedHandlerInvoked { get; set; }

    public bool IncorrectlyAttributedHandlerInvoked { get; set; }
}
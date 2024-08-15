namespace Petl.EventSourcing.Snapshotting;

public sealed class PredicatedSnapshotStrategyOptions
{
    public Func<Type, string, (bool shouldSnapshot, bool shouldTruncate)> ShouldSnapshot { get; set; } = null!;
}
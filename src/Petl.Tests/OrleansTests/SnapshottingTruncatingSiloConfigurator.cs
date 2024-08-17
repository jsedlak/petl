using Petl.EventSourcing.Snapshotting;

namespace Petl.Tests.OrleansTests;

public class SnapshottingTruncatingSiloConfigurator : DefaultSiloConfigurator
{
    public override void Configure(ISiloBuilder siloBuilder)
    {
        base.Configure(siloBuilder);

        siloBuilder.Services.AddPredicatedSnapshotting((type, grainId) => (true, true));
    }
}
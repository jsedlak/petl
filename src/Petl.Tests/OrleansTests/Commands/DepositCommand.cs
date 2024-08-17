using Orleans;

namespace Petl.Tests.OrleansTests.Commands;

[GenerateSerializer]
public class DepositCommand
{
    [Id(0)]
    public double Amount { get; set; }
}
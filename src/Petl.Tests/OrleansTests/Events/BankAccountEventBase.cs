using Orleans;

namespace Petl.Tests.OrleansTests.Events;

[GenerateSerializer]
public abstract class BankAccountEventBase
{
    protected BankAccountEventBase(Guid accountId)
    {
        AccountId = accountId;
    }
    
    [Id(0)]
    public Guid AccountId { get; set; }
}
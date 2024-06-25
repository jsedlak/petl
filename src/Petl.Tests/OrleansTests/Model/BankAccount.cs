using Petl.Tests.OrleansTests.Events;

namespace Petl.Tests.OrleansTests.Model;

public sealed class BankAccount
{
    public Guid Id { get; set; }
    
    public double Balance { get; set; }

    public void Apply(AmountDepositedEvent @event)
    {
        Balance += @event.Amount;
    }

    public void Apply(AmountWithdrawnEvent @event)
    {
        Balance -= @event.Amount;
    }
}
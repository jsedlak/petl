using Petl.EventSourcing;
using Petl.Tests.OrleansTests.Commands;
using Petl.Tests.OrleansTests.Events;
using Petl.Tests.OrleansTests.Model;

namespace Petl.Tests.OrleansTests.Grains;

[PersistTimer(30*1000)]
public class DelayedBankAccountGrain : EventSourcedGrain<BankAccount, BankAccountEventBase>, IDelayedBankAccountGrain
{
    public ValueTask<double> Deposit(DepositCommand command)
    {
        Raise(new AmountDepositedEvent(this.GetPrimaryKey())
        {
            Amount = command.Amount
        });

        return ValueTask.FromResult(TentativeState.Balance);
    }

    public ValueTask<double> Withdraw(WithdrawCommand command)
    {
        var amount = command.Amount;
        if (amount > TentativeState.Balance)
        {
            amount = TentativeState.Balance;
        }

        Raise(new AmountWithdrawnEvent(this.GetPrimaryKey())
        {
            Amount = amount
        });
        
        return ValueTask.FromResult(TentativeState.Balance);
    }

    public ValueTask<double> GetBalance()
    {
        return ValueTask.FromResult(TentativeState.Balance);
    }
}
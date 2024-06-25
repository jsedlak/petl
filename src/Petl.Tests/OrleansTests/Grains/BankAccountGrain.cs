using Orleans;
using Petl.EventSourcing;
using Petl.Tests.OrleansTests.Commands;
using Petl.Tests.OrleansTests.Events;
using Petl.Tests.OrleansTests.Model;

namespace Petl.Tests.OrleansTests.Grains;

public class BankAccountGrain : EventSourcedGrain<BankAccount, BankAccountEventBase>, IBankAccountGrain
{
    public ValueTask<double> Deposit(DepositCommand command)
    {
        Raise(new AmountDepositedEvent(this.GetPrimaryKey())
        {
            Amount = command.Amount
        });

        return ValueTask.FromResult(State.Balance);
    }

    public ValueTask<double> Withdraw(WithdrawCommand command)
    {
        var amount = command.Amount;
        if (amount > State.Balance)
        {
            amount = State.Balance;
        }

        Raise(new AmountWithdrawnEvent(this.GetPrimaryKey())
        {
            Amount = amount
        });
        
        return ValueTask.FromResult(State.Balance);
    }

    public ValueTask<double> GetBalance()
    {
        return ValueTask.FromResult(State.Balance);
    }
}
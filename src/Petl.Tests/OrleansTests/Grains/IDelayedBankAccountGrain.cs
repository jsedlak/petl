using Petl.Tests.OrleansTests.Commands;

namespace Petl.Tests.OrleansTests.Grains;

public interface IDelayedBankAccountGrain : IGrainWithGuidKey
{
    public ValueTask<double> Deposit(DepositCommand command);

    public ValueTask<double> Withdraw(WithdrawCommand command);

    public ValueTask<double> GetBalance();
}
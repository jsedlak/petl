using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Petl.Tests.OrleansTests.Commands;
using Petl.Tests.OrleansTests.Grains;

namespace Petl.Tests.OrleansTests;

[TestClass]
public class BasicEventSourcingTests : OrleansTestBase<DefaultSiloConfigurator>
{
    [TestMethod]
    public async Task CanStoreEvents()
    {
        var bankAccount = Grains.GetGrain<IBankAccountGrain>(Guid.NewGuid());

        var depositBalance = await bankAccount.Deposit(new DepositCommand() { Amount = 2_000 });
        var withdrawBalance = await bankAccount.Withdraw(new WithdrawCommand() { Amount = 1_000 });
        var finalBalance = await bankAccount.GetBalance();

        Assert.AreEqual(2000, depositBalance);
        Assert.AreEqual(finalBalance, withdrawBalance);
    }

    [TestMethod]
    public async Task CanDelayLogWrite()
    {
        var bankAccount = Grains.GetGrain<IDelayedBankAccountGrain>(Guid.NewGuid());

        var depositBalance = await bankAccount.Deposit(new DepositCommand() { Amount = 2_000 });
        var withdrawBalance = await bankAccount.Withdraw(new WithdrawCommand() { Amount = 1_000 });
        var finalBalance = await bankAccount.GetBalance();

        Assert.AreEqual(2000, depositBalance);
        Assert.AreEqual(finalBalance, withdrawBalance);
    }
}
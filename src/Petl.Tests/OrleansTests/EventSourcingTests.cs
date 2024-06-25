using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Petl.EventSourcing;
using Petl.Tests.OrleansTests.Commands;
using Petl.Tests.OrleansTests.Grains;

namespace Petl.Tests.OrleansTests;

[TestClass]
public class EventSourcingTests
{
    private IHost BuildHost()
    {
        var host = Host.CreateDefaultBuilder()
            .UseOrleans(silo =>
            {
                silo
                    .ConfigureServices(services =>
                    {
                        services.AddOrleansEventSerializer();
                        services.AddMemoryEventSourcing();
                    })
                    .UseLocalhostClustering()
                    .UseInMemoryReminderService();
            })
            .Build();

        return host;
    }
    
    [TestMethod]
    public async Task CanStoreEvents()
    {
        Console.WriteLine("Startup");
        
        var host = BuildHost();
        host.RunAsync();
        
        Console.WriteLine("Host Running");

        // var client = Host.CreateDefaultBuilder()
        //     .UseOrleansClient(client => client.UseLocalhostClustering())
        //     .Build();
        //
        // await client.RunAsync();

        var cluster = host.Services.GetRequiredService<IClusterClient>();
        var bankAccount = cluster.GetGrain<IBankAccountGrain>(Guid.NewGuid());

        var depositBalance = await bankAccount.Deposit(new DepositCommand() { Amount = 2_000 });
        var withdrawBalance = await bankAccount.Withdraw(new WithdrawCommand() { Amount = 1_000 });
        var finalBalance = await bankAccount.GetBalance();

        Assert.AreEqual(2000, depositBalance);
        Assert.AreEqual(finalBalance, withdrawBalance);

        // await client.StopAsync();
        host.StopAsync();
    }

    [TestMethod]
    public async Task CanSnapshot()
    {
        
    }

    [TestMethod]
    public async Task CanTruncateLog()
    {
        
    }

    [TestMethod]
    public async Task CanDelayLogWrite()
    {
        
    }
}
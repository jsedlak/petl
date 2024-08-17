using Orleans.TestingHost;
using Petl.EventSourcing;

namespace Petl.Tests.OrleansTests;

public class DefaultSiloConfigurator : ISiloConfigurator
{
    public virtual void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.Services.AddOrleansSerializers();
        siloBuilder.Services.AddMemoryEventSourcing();
        siloBuilder.UseInMemoryReminderService();
    }
}
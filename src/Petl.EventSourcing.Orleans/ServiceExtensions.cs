using Microsoft.Extensions.DependencyInjection;

namespace Petl.EventSourcing;

public static class ServiceExtensions
{
    public static void AddOrleansSerializers(this IServiceCollection services)
    {
        services.AddSingleton<IEventSerializer, OrleansEventSerializer>();
        services.AddSingleton<IStateSerializer, OrleansStateSerializer>();
    }
}
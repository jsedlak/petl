using Microsoft.Extensions.DependencyInjection;

namespace Petl.EventSourcing;

public static class ServiceExtensions
{
    public static void AddOrleansEventSerializer(this IServiceCollection services)
    {
        services.AddSingleton<IEventSerializer, OrleansEventSerializer>();
    }
}
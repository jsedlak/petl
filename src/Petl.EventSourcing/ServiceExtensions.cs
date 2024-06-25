using Microsoft.Extensions.DependencyInjection;

namespace Petl.EventSourcing;

public static class ServiceExtensions
{
    public static void AddMemoryEventSourcing(this IServiceCollection services)
    {
        services.AddScoped<IEventLogFactory, MemoryEventLogFactory>();
    }
}
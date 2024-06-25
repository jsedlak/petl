using Microsoft.Extensions.DependencyInjection;

namespace Petl.EventSourcing.Providers;

public static class ServiceExtensions
{
    public static void AddMongoEventSourcing(this IServiceCollection services, string databaseName)
    {
        services.AddScoped<MongoDbFactorySettings>(sp => new MongoDbFactorySettings { DatabaseName = databaseName });
        services.AddScoped<IEventLogFactory, MongoDbEventLogFactory>();
    }
}
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Petl.EventSourcing.Providers;

public class MongoDbEventLogFactory : IEventLogFactory
{
    private static readonly Regex SanitizeExpression = new Regex("[^a-zA-Z0-9 -]");

    private readonly MongoDbFactorySettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MongoDbEventLogFactory> _logger;
    
    public MongoDbEventLogFactory(MongoDbFactorySettings settings, IServiceProvider serviceProvider, ILogger<MongoDbEventLogFactory> logger)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public IEventLog<TView, TEntry> Create<TView, TEntry>(Type grainType, string viewId) 
        where TView : class, new() 
        where TEntry : class
    {
        _logger.LogInformation($"MongoDbEventLog Requested for {grainType.Name} and ID {viewId}");
        
        // grab some services
        var mongoClient = _serviceProvider.GetRequiredService<IMongoClient>();
        var eventSerializer = _serviceProvider.GetRequiredService<IEventSerializer>();
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<MongoDbEventLog<TView, TEntry>>();
        
        // TODO: We probably want to move this into an injectable
        
        // grab the name of the grain, falling back to the type name
        var grainName = viewId.IndexOf("/", StringComparison.OrdinalIgnoreCase) > 0
            ? viewId.Split(["/"], StringSplitOptions.RemoveEmptyEntries).First().Trim() :
              SanitizeExpression.Replace(grainType.Name, "");

        // grab the identifier. for orleans grains this will be the second part
        var grainId = viewId.IndexOf("/", StringComparison.OrdinalIgnoreCase) > 0
            ? viewId.Substring(viewId.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1) :
            viewId;

        var settings = new MongoDbEventLogSettings
        {
            DatabaseName = _settings.DatabaseName,
            GrainType = grainName,
            GrainId = grainId
        };

        return new MongoDbEventLog<TView, TEntry>(settings, mongoClient, eventSerializer, logger);
    }
}

public static class ServiceExtensions
{
    public static void AddMongoEventSourcing(this IServiceCollection services, string databaseName)
    {
        services.AddScoped<MongoDbFactorySettings>(sp => new MongoDbFactorySettings { DatabaseName = databaseName });
        services.AddScoped<IEventLogFactory, MongoDbEventLogFactory>();
    }
}
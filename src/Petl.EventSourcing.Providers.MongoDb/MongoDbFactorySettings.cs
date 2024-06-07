namespace Petl.EventSourcing.Providers;

public record MongoDbFactorySettings
{
    public string DatabaseName { get; set; } = null!;
}
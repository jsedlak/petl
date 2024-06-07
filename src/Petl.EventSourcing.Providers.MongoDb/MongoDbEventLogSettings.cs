namespace Petl.EventSourcing.Providers;

public record MongoDbEventLogSettings
{
    public string DatabaseName { get; set; } = null!;
    
    public string GrainType { get; set; } = null!;

    public string GrainId { get; set; } = null!;
}
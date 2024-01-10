namespace Petl;

public class RequestContext 
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? UserIdentifier { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
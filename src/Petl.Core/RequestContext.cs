namespace Petl;

public class RequestContext 
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
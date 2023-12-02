namespace Petl;

public class CommandContext 
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
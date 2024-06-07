namespace Petl.Mediator;

public class ResponseContext 
{
    public Guid CorrelationId { get; set; }

    public string? UserIdentfier { get; set; }
}
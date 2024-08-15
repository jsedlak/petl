namespace Petl.EventSourcing.Providers;

internal class Snapshot<TView> where TView : class
{
    public Guid Id { get; set; }

    public string GrainId { get; set; } = null!;

    public TView View { get; set; } = null!;
    
    public int Version { get; set; }
}
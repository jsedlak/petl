namespace Petl.EventSourcing;

[AttributeUsage(AttributeTargets.Class)]
public class PersistTimerAttribute : Attribute
{
    public static readonly TimeSpan DefaultTime = TimeSpan.FromSeconds(15);

    public PersistTimerAttribute()
        : this(DefaultTime)
    {

    }
    

    public PersistTimerAttribute(TimeSpan time)
    {
        Time = time;
    }
    
    public TimeSpan Time { get; set; }
}
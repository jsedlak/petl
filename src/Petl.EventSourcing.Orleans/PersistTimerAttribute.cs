namespace Petl.EventSourcing;

[AttributeUsage(AttributeTargets.Class)]
public class PersistTimerAttribute : Attribute
{
    public static readonly TimeSpan DefaultTime = TimeSpan.FromSeconds(15);

    public PersistTimerAttribute()
        : this(DefaultTime.Seconds)
    {

    }
    

    public PersistTimerAttribute(int timeInSeconds)
    {
        Time = TimeSpan.FromSeconds(timeInSeconds);
    }
    
    public TimeSpan Time { get; set; }
}
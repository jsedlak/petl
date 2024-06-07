namespace Petl.Mediator;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ResponseDispatcherAttribute : Attribute
{
    public ResponseDispatcherAttribute(Type dispatcherType)
    {
        DispatcherType = dispatcherType;
    }

    public Type DispatcherType { get; }
}
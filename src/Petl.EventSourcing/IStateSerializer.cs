namespace Petl.EventSourcing;

public interface IStateSerializer
{
    BinaryData Serialize<TView>(TView data);
    
    TView Deserialize<TView>(byte[] data);
}
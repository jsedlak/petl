using Petl.Converters;

namespace Petl.Builder
{
    public interface ICanAddConverter
    {
        ICanAddConverter AddConverter(IValueConverter valueConverter);
    }
}

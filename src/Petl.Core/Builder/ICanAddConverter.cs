using Petl.Converters;

namespace Petl.Builder
{
    public interface ICanAddConverter<TInput, TOutput>
    {
        void AddConverter(IValueConverter valueConverter);
    }
}

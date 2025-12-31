using System.Reflection;

namespace Petl;

/// <summary>
/// Represents an auto-mapped property transformation that copies matching properties
/// from source to target objects
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
internal class AutoMapTransformation<TSource, TTarget> : ITransformationStep
{
    private readonly List<(PropertyInfo Source, PropertyInfo Target)> _propertyMappings;
    private readonly Func<TSource, TTarget, object?, bool>? _filter;

    /// <summary>
    /// Initializes a new instance of the AutoMapTransformation class
    /// </summary>
    /// <param name="propertyMappings">The matched property pairs to copy</param>
    /// <param name="filter">Optional filter callback to determine if a value should be copied</param>
    public AutoMapTransformation(
        IEnumerable<(PropertyInfo Source, PropertyInfo Target)> propertyMappings,
        Func<TSource, TTarget, object?, bool>? filter = null)
    {
        _propertyMappings = propertyMappings?.ToList() ?? throw new ArgumentNullException(nameof(propertyMappings));
        _filter = filter;
    }

    /// <summary>
    /// Executes the auto-map transformation
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="target">The target object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public Task Execute(object source, object target, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (source is not TSource typedSource || target is not TTarget typedTarget)
        {
            return Task.CompletedTask;
        }

        foreach (var (sourceProp, targetProp) in _propertyMappings)
        {
            var value = sourceProp.GetValue(typedSource);

            // Apply filter if provided
            if (_filter != null && !_filter(typedSource, typedTarget, value))
            {
                continue;
            }

            // Copy value to target
            SetTargetValue(typedTarget, targetProp, value);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets a value on the target property, handling null values appropriately
    /// </summary>
    private static void SetTargetValue(TTarget target, PropertyInfo targetProp, object? value)
    {
        var targetType = targetProp.PropertyType;

        if (value != null)
        {
            var sourceType = value.GetType();

            // If types match exactly, set directly
            if (targetType == sourceType)
            {
                targetProp.SetValue(target, value);
            }
            // If target is nullable and source is the underlying type
            else if (targetType.IsGenericType &&
                     targetType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                     targetType.GetGenericArguments()[0] == sourceType)
            {
                targetProp.SetValue(target, value);
            }
            // If source is nullable and target is the underlying type
            else if (sourceType.IsGenericType &&
                     sourceType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                     sourceType.GetGenericArguments()[0] == targetType)
            {
                targetProp.SetValue(target, value);
            }
            // Try assignable types
            else if (targetType.IsAssignableFrom(sourceType))
            {
                targetProp.SetValue(target, value);
            }
        }
        else
        {
            // Handle null values - only set if target property is nullable
            if (targetType.IsClass || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                targetProp.SetValue(target, null);
            }
        }
    }
}

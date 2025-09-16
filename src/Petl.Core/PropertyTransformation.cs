using System.Linq.Expressions;
using System.Reflection;

namespace Petl;

/// <summary>
/// Represents a property-to-property transformation
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
public class PropertyTransformation<TSource, TTarget> : ITransformationStep
{
    private readonly Func<TSource, object?> _sourceProperty;
    private readonly PropertyInfo _targetProperty;

    /// <summary>
    /// Initializes a new instance of the PropertyTransformation class
    /// </summary>
    /// <param name="sourceProperty">Expression to get the source property value</param>
    /// <param name="targetProperty">Expression to set the target property value</param>
    public PropertyTransformation(
        Expression<Func<TSource, object?>> sourceProperty,
        Expression<Func<TTarget, object?>> targetProperty)
    {
        _sourceProperty = sourceProperty.Compile();
        
        // Extract the target property info
        // Handle both direct property access and unary expressions (for value types)
        Expression body = targetProperty.Body;
        if (body is UnaryExpression unaryExpression)
        {
            body = unaryExpression.Operand;
        }
        
        if (body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
        {
            _targetProperty = propertyInfo;
        }
        else
        {
            throw new ArgumentException("Target expression must be a property access", nameof(targetProperty));
        }
    }

    /// <summary>
    /// Executes the property transformation
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="target">The target object</param>
    public void Execute(object source, object target)
    {
        if (source is TSource sourceObj && target is TTarget targetObj)
        {
            var value = _sourceProperty(sourceObj);
            
            // Handle value type conversions properly
            var targetType = _targetProperty.PropertyType;
            
            if (value != null)
            {
                var sourceType = value.GetType();
                
                // If types match exactly, set directly
                if (targetType == sourceType)
                {
                    _targetProperty.SetValue(targetObj, value);
                }
                // If target is nullable and source is the underlying type
                else if (targetType.IsGenericType && 
                         targetType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                         targetType.GetGenericArguments()[0] == sourceType)
                {
                    _targetProperty.SetValue(targetObj, value);
                }
                // If source is nullable and target is the underlying type
                else if (sourceType.IsGenericType && 
                         sourceType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                         sourceType.GetGenericArguments()[0] == targetType)
                {
                    _targetProperty.SetValue(targetObj, value);
                }
                // Try to convert using Convert.ChangeType
                else if (targetType.IsAssignableFrom(sourceType))
                {
                    _targetProperty.SetValue(targetObj, value);
                }
                else
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, targetType);
                        _targetProperty.SetValue(targetObj, convertedValue);
                    }
                    catch
                    {
                        // If conversion fails, try to set as-is (might work for some cases)
                        _targetProperty.SetValue(targetObj, value);
                    }
                }
            }
            else
            {
                // Handle null values - only set if target property is nullable
                if (targetType.IsClass || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    _targetProperty.SetValue(targetObj, null);
                }
            }
        }
    }
}

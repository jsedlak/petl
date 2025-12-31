using System.Reflection;

namespace Petl;

/// <summary>
/// Builder for creating transformation pipelines
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
public class PipelineBuilder<TSource, TTarget>
{
    private readonly List<TransformationStep<TSource, TTarget>> _steps;

    /// <summary>
    /// Initializes a new instance of the PipelineBuilder class
    /// </summary>
    public PipelineBuilder()
    {
        _steps = new List<TransformationStep<TSource, TTarget>>();
    }

    /// <summary>
    /// Adds a new transformation step to the pipeline
    /// </summary>
    /// <param name="stepName">The name of the transformation step</param>
    /// <returns>A transformation step builder for method chaining</returns>
    public TransformationStep<TSource, TTarget> WithStep(string stepName)
    {
        var step = new TransformationStep<TSource, TTarget>(stepName);
        _steps.Add(step);
        return step;
    }

    /// <summary>
    /// Adds an auto-map step that automatically maps properties with matching names and types
    /// </summary>
    /// <param name="stepName">The name of the transformation step (defaults to "AutoMap")</param>
    /// <returns>This builder for method chaining</returns>
    public PipelineBuilder<TSource, TTarget> WithAutoMapStep(string stepName = "AutoMap")
    {
        return WithAutoMapStep(null, stepName);
    }

    /// <summary>
    /// Adds an auto-map step with a filter callback to conditionally copy values
    /// </summary>
    /// <param name="filter">Optional filter callback that receives source, target, and value; returns true to copy</param>
    /// <param name="stepName">The name of the transformation step (defaults to "AutoMap")</param>
    /// <returns>This builder for method chaining</returns>
    public PipelineBuilder<TSource, TTarget> WithAutoMapStep(
        Func<TSource, TTarget, object?, bool>? filter,
        string stepName = "AutoMap")
    {
        var propertyMappings = GetMatchingProperties();
        var autoMapTransformation = new AutoMapTransformation<TSource, TTarget>(propertyMappings, filter);

        var step = new TransformationStep<TSource, TTarget>(stepName);
        step.AddTransformation(autoMapTransformation);
        _steps.Add(step);

        return this;
    }

    /// <summary>
    /// Gets all matching properties between source and target types
    /// </summary>
    private static IEnumerable<(PropertyInfo Source, PropertyInfo Target)> GetMatchingProperties()
    {
        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p);

        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var targetProp in targetProperties)
        {
            if (sourceProperties.TryGetValue(targetProp.Name, out var sourceProp))
            {
                if (AreTypesCompatible(sourceProp.PropertyType, targetProp.PropertyType))
                {
                    yield return (sourceProp, targetProp);
                }
            }
        }
    }

    /// <summary>
    /// Determines if source type can be assigned to target type
    /// </summary>
    private static bool AreTypesCompatible(Type sourceType, Type targetType)
    {
        // Exact match
        if (sourceType == targetType)
        {
            return true;
        }

        // Target is assignable from source (inheritance, interfaces)
        if (targetType.IsAssignableFrom(sourceType))
        {
            return true;
        }

        // Handle nullable to non-nullable and vice versa
        var underlyingSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingSource == underlyingTarget)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Builds the transformation pipeline
    /// </summary>
    /// <returns>A pipeline that can execute transformations</returns>
    public Pipeline<TSource, TTarget> Build()
    {
        return new Pipeline<TSource, TTarget>(
            _steps.Cast<ITransformationStepContainer<TSource, TTarget>>());
    }
}

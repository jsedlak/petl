using System.Linq.Expressions;

namespace Petl;

/// <summary>
/// Represents a step in the transformation pipeline
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
public class TransformationStep<TSource, TTarget> : ITransformationStepContainer<TSource, TTarget>
{
    private readonly string _stepName;
    private readonly List<ITransformationStep> _transformations;

    /// <summary>
    /// Initializes a new instance of the TransformationStep class
    /// </summary>
    /// <param name="stepName">The name of the transformation step</param>
    public TransformationStep(string stepName)
    {
        _stepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
        _transformations = new List<ITransformationStep>();
    }

    /// <summary>
    /// Adds a property transformation to this step
    /// </summary>
    /// <param name="sourceProperty">Expression to get the source property value</param>
    /// <param name="targetProperty">Expression to set the target property value</param>
    /// <returns>This transformation step for method chaining</returns>
    public TransformationStep<TSource, TTarget> Property(
        Expression<Func<TSource, object?>> sourceProperty,
        Expression<Func<TTarget, object?>> targetProperty)
    {
        _transformations.Add(new PropertyTransformation<TSource, TTarget>(sourceProperty, targetProperty));
        return this;
    }

    /// <summary>
    /// Adds a synchronous custom transformation to this step
    /// </summary>
    /// <param name="transformAction">Sync transformation action</param>
    /// <returns>This transformation step for method chaining</returns>
    public TransformationStep<TSource, TTarget> Transform(Action<TSource, TTarget> transformAction)
    {
        _transformations.Add(new TransformTransformation<TSource, TTarget>(transformAction));
        return this;
    }

    /// <summary>
    /// Adds an asynchronous custom transformation to this step
    /// </summary>
    /// <param name="transformAction">Async transformation action</param>
    /// <returns>This transformation step for method chaining</returns>
    public TransformationStep<TSource, TTarget> Transform(Func<TSource, TTarget, Task> transformAction)
    {
        _transformations.Add(new TransformTransformation<TSource, TTarget>(transformAction));
        return this;
    }

    /// <summary>
    /// Adds an asynchronous custom transformation with cancellation support to this step
    /// </summary>
    /// <param name="transformAction">Async transformation action with cancellation token</param>
    /// <returns>This transformation step for method chaining</returns>
    public TransformationStep<TSource, TTarget> Transform(
        Func<TSource, TTarget, CancellationToken, Task> transformAction)
    {
        _transformations.Add(new TransformTransformation<TSource, TTarget>(transformAction));
        return this;
    }

    /// <summary>
    /// Adds an existing transformation step to this step (internal use)
    /// </summary>
    /// <param name="transformation">The transformation to add</param>
    internal void AddTransformation(ITransformationStep transformation)
    {
        _transformations.Add(transformation);
    }

    /// <summary>
    /// Executes all transformations in this step
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="target">The target object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    async Task ITransformationStepContainer<TSource, TTarget>.Execute(
        TSource source, TTarget target, CancellationToken cancellationToken)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        foreach (var transformation in _transformations)
        {
            await transformation.Execute(source, target, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the name of this transformation step
    /// </summary>
    public string StepName => _stepName;
}

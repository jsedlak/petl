namespace Petl;

/// <summary>
/// Represents a container for transformation steps
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
internal interface ITransformationStepContainer<TSource, TTarget>
{
    /// <summary>
    /// Gets the name of this step
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Executes all transformations in this step
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="target">The target object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task Execute(TSource source, TTarget target, CancellationToken cancellationToken = default);
}


namespace Petl;

/// <summary>
/// Represents a transformation pipeline that can execute data transformations
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
public interface IPipeline<TSource, TTarget>
{
    /// <summary>
    /// Executes the transformation pipeline
    /// </summary>
    /// <param name="source">The source object to transform from</param>
    /// <param name="target">The target object to transform to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task Exec(TSource source, TTarget target, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of transformation steps in this pipeline
    /// </summary>
    int StepCount { get; }

    /// <summary>
    /// Gets the names of all transformation steps in this pipeline
    /// </summary>
    IEnumerable<string> StepNames { get; }
}

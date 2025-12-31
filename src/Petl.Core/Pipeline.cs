namespace Petl;

/// <summary>
/// Represents a transformation pipeline that can execute data transformations
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
public class Pipeline<TSource, TTarget> : IPipeline<TSource, TTarget>
{
    private readonly List<ITransformationStepContainer<TSource, TTarget>> _steps;

    /// <summary>
    /// Initializes a new instance of the Pipeline class
    /// </summary>
    /// <param name="steps">The transformation steps to execute</param>
    internal Pipeline(IEnumerable<ITransformationStepContainer<TSource, TTarget>> steps)
    {
        _steps = steps?.ToList() ?? throw new ArgumentNullException(nameof(steps));
    }

    /// <summary>
    /// Executes the transformation pipeline
    /// </summary>
    /// <param name="source">The source object to transform from</param>
    /// <param name="target">The target object to transform to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public async Task Exec(TSource source, TTarget target, CancellationToken cancellationToken = default)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        foreach (var step in _steps)
        {
            await step.Execute(source, target, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the number of transformation steps in this pipeline
    /// </summary>
    public int StepCount => _steps.Count;

    /// <summary>
    /// Gets the names of all transformation steps in this pipeline
    /// </summary>
    public IEnumerable<string> StepNames => _steps.Select(s => s.StepName);
}

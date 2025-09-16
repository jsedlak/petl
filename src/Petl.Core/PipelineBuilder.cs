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
    /// Builds the transformation pipeline
    /// </summary>
    /// <returns>A pipeline that can execute transformations</returns>
    public Pipeline<TSource, TTarget> Build()
    {
        return new Pipeline<TSource, TTarget>(_steps);
    }
}

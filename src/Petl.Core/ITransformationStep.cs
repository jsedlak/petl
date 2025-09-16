namespace Petl;

/// <summary>
/// Represents a single transformation step in a pipeline
/// </summary>
public interface ITransformationStep
{
    /// <summary>
    /// Executes the transformation step
    /// </summary>
    /// <param name="source">The input object</param>
    /// <param name="target">The output object</param>
    void Execute(object source, object target);
}

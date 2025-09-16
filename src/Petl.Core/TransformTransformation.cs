namespace Petl;

/// <summary>
/// Represents a custom transformation with a callback handler
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
public class TransformTransformation<TSource, TTarget> : ITransformationStep
{
    private readonly Action<TSource, TTarget> _transformAction;

    /// <summary>
    /// Initializes a new instance of the TransformTransformation class
    /// </summary>
    /// <param name="transformAction">The transformation action to execute</param>
    public TransformTransformation(Action<TSource, TTarget> transformAction)
    {
        _transformAction = transformAction ?? throw new ArgumentNullException(nameof(transformAction));
    }

    /// <summary>
    /// Executes the custom transformation
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="target">The target object</param>
    public void Execute(object source, object target)
    {
        if (source is TSource sourceObj && target is TTarget targetObj)
        {
            _transformAction(sourceObj, targetObj);
        }
    }
}

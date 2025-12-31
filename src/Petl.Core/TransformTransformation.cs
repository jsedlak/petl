namespace Petl;

/// <summary>
/// Represents a custom transformation with a callback handler
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
internal class TransformTransformation<TSource, TTarget> : ITransformationStep
{
    private readonly Func<TSource, TTarget, CancellationToken, Task> _transformAction;

    /// <summary>
    /// Initializes with a synchronous action (no cancellation token)
    /// </summary>
    /// <param name="transformAction">The transformation action to execute</param>
    public TransformTransformation(Action<TSource, TTarget> transformAction)
    {
        if (transformAction == null)
        {
            throw new ArgumentNullException(nameof(transformAction));
        }

        _transformAction = (source, target, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            transformAction(source, target);
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Initializes with an async function (no cancellation token)
    /// </summary>
    /// <param name="transformAction">The async transformation action to execute</param>
    public TransformTransformation(Func<TSource, TTarget, Task> transformAction)
    {
        if (transformAction == null)
        {
            throw new ArgumentNullException(nameof(transformAction));
        }

        _transformAction = async (source, target, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            await transformAction(source, target);
        };
    }

    /// <summary>
    /// Initializes with an async function (with cancellation token)
    /// </summary>
    /// <param name="transformAction">The async transformation action with cancellation support</param>
    public TransformTransformation(Func<TSource, TTarget, CancellationToken, Task> transformAction)
    {
        _transformAction = transformAction ?? throw new ArgumentNullException(nameof(transformAction));
    }

    /// <summary>
    /// Executes the custom transformation
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="target">The target object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    public async Task Execute(object source, object target, CancellationToken cancellationToken = default)
    {
        if (source is TSource typedSource && target is TTarget typedTarget)
        {
            await _transformAction(typedSource, typedTarget, cancellationToken);
        }
    }
}

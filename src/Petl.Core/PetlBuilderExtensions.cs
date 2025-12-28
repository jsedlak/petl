using Microsoft.Extensions.DependencyInjection;

namespace Petl;

/// <summary>
/// Extension methods for configuring pipelines on PetlBuilder
/// </summary>
public static class PetlBuilderExtensions
{
    /// <summary>
    /// Registers a pipeline as a singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="configure">Action to configure the pipeline</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithPipeline<TSource, TTarget>(
        this PetlBuilder builder,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        configure(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddSingleton<IPipeline<TSource, TTarget>>(pipeline);
        return builder;
    }

    /// <summary>
    /// Registers a pipeline as a keyed singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="name">The service key name</param>
    /// <param name="configure">Action to configure the pipeline</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithPipeline<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        configure(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddKeyedSingleton<IPipeline<TSource, TTarget>>(name, pipeline);
        return builder;
    }

    /// <summary>
    /// Registers an auto-mapped pipeline as a singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder)
    {
        return WithAutoMapping<TSource, TTarget>(builder, filter: null, configure: null);
    }

    /// <summary>
    /// Registers an auto-mapped pipeline with additional configuration as a singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="configure">Action to add additional pipeline steps after auto-mapping</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        return WithAutoMapping<TSource, TTarget>(builder, filter: null, configure: configure);
    }

    /// <summary>
    /// Registers an auto-mapped pipeline with a filter as a singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="filter">Filter callback to determine if a value should be copied</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        Func<TSource, TTarget, object?, bool> filter)
    {
        return WithAutoMapping<TSource, TTarget>(builder, filter: filter, configure: null);
    }

    /// <summary>
    /// Registers an auto-mapped pipeline with a filter and additional configuration as a singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="filter">Filter callback to determine if a value should be copied</param>
    /// <param name="configure">Action to add additional pipeline steps after auto-mapping</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        Func<TSource, TTarget, object?, bool>? filter,
        Action<PipelineBuilder<TSource, TTarget>>? configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        pipelineBuilder.WithAutoMapStep(filter);
        configure?.Invoke(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddSingleton<IPipeline<TSource, TTarget>>(pipeline);
        return builder;
    }

    /// <summary>
    /// Registers an auto-mapped pipeline as a keyed singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="name">The service key name</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name)
    {
        return WithAutoMapping<TSource, TTarget>(builder, name, filter: null, configure: null);
    }

    /// <summary>
    /// Registers an auto-mapped pipeline with additional configuration as a keyed singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="name">The service key name</param>
    /// <param name="configure">Action to add additional pipeline steps after auto-mapping</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        return WithAutoMapping<TSource, TTarget>(builder, name, filter: null, configure: configure);
    }

    /// <summary>
    /// Registers an auto-mapped pipeline with a filter as a keyed singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="name">The service key name</param>
    /// <param name="filter">Filter callback to determine if a value should be copied</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Func<TSource, TTarget, object?, bool> filter)
    {
        return WithAutoMapping<TSource, TTarget>(builder, name, filter: filter, configure: null);
    }

    /// <summary>
    /// Registers an auto-mapped pipeline with a filter and additional configuration as a keyed singleton service
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="builder">The Petl builder</param>
    /// <param name="name">The service key name</param>
    /// <param name="filter">Filter callback to determine if a value should be copied</param>
    /// <param name="configure">Action to add additional pipeline steps after auto-mapping</param>
    /// <returns>The Petl builder for chaining</returns>
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Func<TSource, TTarget, object?, bool>? filter,
        Action<PipelineBuilder<TSource, TTarget>>? configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        pipelineBuilder.WithAutoMapStep(filter);
        configure?.Invoke(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddKeyedSingleton<IPipeline<TSource, TTarget>>(name, pipeline);
        return builder;
    }
}


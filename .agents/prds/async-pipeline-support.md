# Async Pipeline Support PRD

Refactor the transformation pipeline to be async-first throughout. All execution methods return `Task` and support `CancellationToken`. Method and interface names do not use "Async" suffix - async is the default behavior.

## Overview

- Update `ITransformationStep.Execute` to return `Task`
- Update `IPipeline.Exec` to return `Task`
- Add `Transform` overload accepting `Func<TSource, TTarget, CancellationToken, Task>`
- Update all existing transformation implementations to be async
- Update `Pipeline` constructor to accept interfaces instead of concrete types
- Maintain backward compatibility by wrapping sync delegates in `Task.CompletedTask`

---

# ITransformationStep Interface Updates

## Update: `src/Petl.Core/ITransformationStep.cs`

- [ ] Change `Execute` to return `Task`
- [ ] Add `CancellationToken` parameter

```csharp
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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task Execute(object source, object target, CancellationToken cancellationToken = default);
}
```

---

# IPipeline Interface Updates

## Update: `src/Petl.Core/IPipeline.cs`

- [ ] Change `Exec` to return `Task`
- [ ] Add `CancellationToken` parameter

```csharp
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
```

---

# PropertyTransformation Updates

## Update: `src/Petl.Core/PropertyTransformation.cs`

- [ ] Update `Execute` to return `Task`
- [ ] Add `CancellationToken` parameter
- [ ] Return `Task.CompletedTask` after sync operation

```csharp
public Task Execute(object source, object target, CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();

    if (source is TSource sourceObj && target is TTarget targetObj)
    {
        // ... existing property copy logic ...
    }

    return Task.CompletedTask;
}
```

---

# TransformTransformation Updates

## Update: `src/Petl.Core/TransformTransformation.cs`

- [ ] Store delegate as `Func<TSource, TTarget, CancellationToken, Task>` internally
- [ ] Update `Execute` to return `Task`
- [ ] Support three constructor overloads for seamless sync/async handling

```csharp
namespace Petl;

/// <summary>
/// Represents a custom transformation
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target type</typeparam>
internal class TransformTransformation<TSource, TTarget> : ITransformationStep
{
    private readonly Func<TSource, TTarget, CancellationToken, Task> _transformAction;

    /// <summary>
    /// Initializes with a synchronous action (no cancellation token)
    /// </summary>
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
    public TransformTransformation(Func<TSource, TTarget, CancellationToken, Task> transformAction)
    {
        _transformAction = transformAction ?? throw new ArgumentNullException(nameof(transformAction));
    }

    public async Task Execute(object source, object target, CancellationToken cancellationToken = default)
    {
        if (source is TSource typedSource && target is TTarget typedTarget)
        {
            await _transformAction(typedSource, typedTarget, cancellationToken);
        }
    }
}
```

---

# AutoMapTransformation Updates

## Update: `src/Petl.Core/AutoMapTransformation.cs`

- [ ] Update `Execute` to return `Task`
- [ ] Add `CancellationToken` parameter
- [ ] Return `Task.CompletedTask` after sync operation

```csharp
public Task Execute(object source, object target, CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();

    // ... existing auto-map logic ...

    return Task.CompletedTask;
}
```

---

# ITransformationStepContainer Interface

## New File: `src/Petl.Core/ITransformationStepContainer.cs`

- [ ] Create interface to abstract `TransformationStep` for Pipeline constructor

```csharp
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
```

---

# TransformationStep Updates

## Update: `src/Petl.Core/TransformationStep.cs`

- [ ] Implement `ITransformationStepContainer<TSource, TTarget>`
- [ ] Add three `Transform` overloads for seamless sync/async handling
- [ ] Update internal `Execute` to be async

```csharp
public class TransformationStep<TSource, TTarget> : ITransformationStepContainer<TSource, TTarget>
{
    private readonly string _stepName;
    private readonly List<ITransformationStep> _transformations;

    public TransformationStep(string stepName)
    {
        _stepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
        _transformations = new List<ITransformationStep>();
    }

    /// <summary>
    /// Adds a property transformation to this step
    /// </summary>
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
    public TransformationStep<TSource, TTarget> Transform(Action<TSource, TTarget> transformAction)
    {
        _transformations.Add(new TransformTransformation<TSource, TTarget>(transformAction));
        return this;
    }

    /// <summary>
    /// Adds an asynchronous custom transformation to this step
    /// </summary>
    /// <param name="transformAction">Async transformation action</param>
    public TransformationStep<TSource, TTarget> Transform(Func<TSource, TTarget, Task> transformAction)
    {
        _transformations.Add(new TransformTransformation<TSource, TTarget>(transformAction));
        return this;
    }

    /// <summary>
    /// Adds an asynchronous custom transformation with cancellation support to this step
    /// </summary>
    /// <param name="transformAction">Async transformation action with cancellation token</param>
    public TransformationStep<TSource, TTarget> Transform(
        Func<TSource, TTarget, CancellationToken, Task> transformAction)
    {
        _transformations.Add(new TransformTransformation<TSource, TTarget>(transformAction));
        return this;
    }

    /// <summary>
    /// Adds an existing transformation step to this step (internal use)
    /// </summary>
    internal void AddTransformation(ITransformationStep transformation)
    {
        _transformations.Add(transformation);
    }

    /// <summary>
    /// Executes all transformations in this step
    /// </summary>
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
```

---

# Pipeline Updates

## Update: `src/Petl.Core/Pipeline.cs`

- [ ] Change constructor to accept `IEnumerable<ITransformationStepContainer<TSource, TTarget>>`
- [ ] Update `Exec` to be async and return `Task`

```csharp
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
```

---

# PipelineBuilder Updates

## Update: `src/Petl.Core/PipelineBuilder.cs`

- [ ] Update `Build()` to cast steps to interface type

```csharp
public Pipeline<TSource, TTarget> Build()
{
    return new Pipeline<TSource, TTarget>(
        _steps.Cast<ITransformationStepContainer<TSource, TTarget>>());
}
```

---

# Unit Tests

## Update: `src/Petl.Tests/PipelineTests.cs`

- [ ] Update all existing tests to use `await pipeline.Exec(...)`
- [ ] Add `[TestMethod]` to `async Task` methods
- [ ] Add tests for cancellation token support
- [ ] Add tests for async Transform overload

### Example Updated Test

```csharp
[TestMethod]
public async Task Pipeline_Exec_ShouldExecutePropertyTransformation()
{
    // Arrange
    var builder = new PipelineBuilder<InputModel, OutputModel>();
    builder
        .WithStep("Property Transform")
            .Property(x => x.SourceProperty, y => y.TargetProperty);

    var pipeline = builder.Build();
    var input = new InputModel { SourceProperty = "Hello World" };
    var output = new OutputModel();

    // Act
    await pipeline.Exec(input, output);

    // Assert
    Assert.AreEqual("Hello World", output.TargetProperty);
}
```

### New Async-Specific Tests

```csharp
[TestMethod]
public async Task Pipeline_Exec_ShouldExecuteAsyncTransformation()
{
    // Arrange
    var builder = new PipelineBuilder<TestInput, TestOutput>();
    builder.WithStep("Async Step")
        // Async without cancellation token - just works!
        .Transform(async (source, target) =>
        {
            await Task.Delay(10);
            target.FullName = source.Name.ToUpper();
        });

    var pipeline = builder.Build();
    var input = new TestInput { Name = "test" };
    var output = new TestOutput();

    // Act
    await pipeline.Exec(input, output);

    // Assert
    Assert.AreEqual("TEST", output.FullName);
}

[TestMethod]
public async Task Pipeline_Exec_ShouldSupportCancellation()
{
    // Arrange
    var builder = new PipelineBuilder<TestInput, TestOutput>();
    builder.WithStep("Cancellable Step")
        .Transform(async (source, target, ct) =>
        {
            await Task.Delay(1000, ct);
            target.FullName = source.Name;
        });

    var pipeline = builder.Build();
    var input = new TestInput { Name = "test" };
    var output = new TestOutput();
    var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act & Assert
    await Assert.ThrowsExceptionAsync<OperationCanceledException>(
        () => pipeline.Exec(input, output, cts.Token));
}

[TestMethod]
public async Task Pipeline_Exec_ShouldExecuteMixedSyncAndAsyncTransformations()
{
    // Arrange
    var builder = new PipelineBuilder<TestInput, TestOutput>();
    builder.WithStep("Mixed Step")
        // Sync - no async keyword
        .Transform((source, target) => target.FullName = source.Name)
        // Async without cancellation token
        .Transform(async (source, target) =>
        {
            await Task.Delay(5);
            target.BirthYear = "2000";
        })
        // Async with cancellation token
        .Transform(async (source, target, ct) =>
        {
            await Task.Delay(10, ct);
            target.Description = $"Processed: {target.FullName}";
        });

    var pipeline = builder.Build();
    var input = new TestInput { Name = "Test" };
    var output = new TestOutput();

    // Act
    await pipeline.Exec(input, output);

    // Assert
    Assert.AreEqual("Test", output.FullName);
    Assert.AreEqual("2000", output.BirthYear);
    Assert.AreEqual("Processed: Test", output.Description);
}
```

## Update: `src/Petl.Tests/DependencyInjectionTests.cs`

- [ ] Update all tests to use `await pipeline.Exec(...)`

---

# README Updates

## Update: `README.md`

### Update Quick Start Examples

- [ ] Update all `pipeline.Exec(...)` to `await pipeline.Exec(...)`

### Update Dependency Injection Examples

- [ ] Update service examples to show async usage

```csharp
public class UserService
{
    private readonly IPipeline<UserDto, UserViewModel> _pipeline;

    public UserService(IPipeline<UserDto, UserViewModel> pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<UserViewModel> ToViewModelAsync(UserDto dto, CancellationToken ct = default)
    {
        var viewModel = new UserViewModel();
        await _pipeline.Exec(dto, viewModel, ct);
        return viewModel;
    }
}
```

### Add Async Transform Section

````markdown
### Async Transformations

The `Transform` method seamlessly handles both sync and async delegates - just write your lambda naturally:

```csharp
var pipeline = new PipelineBuilder<UserDto, UserViewModel>()
    .WithAutoMapStep()
    .WithStep("Process")
        // Sync - just works
        .Transform((source, target) => target.Name = source.Name.Trim())

        // Async - just works
        .Transform(async (source, target) =>
        {
            target.Details = await userService.GetDetailsAsync(source.Id);
        })

        // Async with cancellation - just works
        .Transform(async (source, target, cancellationToken) =>
        {
            target.Extra = await api.FetchAsync(source.Id, cancellationToken);
        })
    .Build();

// Always await Exec
await pipeline.Exec(dto, viewModel, cancellationToken);
```

All three styles can be mixed freely in the same step. The pipeline handles them uniformly.
````

### Update API Reference

- [ ] Update `IPipeline` table to show `Task Exec(...)`
- [ ] Update `TransformationStep` table to show all three Transform overloads

```markdown
### IPipeline<TSource, TTarget>

| Property/Method                                  | Description                                |
| ------------------------------------------------ | ------------------------------------------ |
| `Task Exec(TSource, TTarget, CancellationToken)` | Executes the transformation pipeline       |
| `StepCount`                                      | Gets the number of transformation steps    |
| `StepNames`                                      | Gets the names of all transformation steps |

### TransformationStep<TSource, TTarget>

| Method                                                       | Description                                            |
| ------------------------------------------------------------ | ------------------------------------------------------ |
| `Property(...)`                                              | Maps a source property to a target property            |
| `Transform(Action<TSource, TTarget>)`                        | Adds a sync transformation                             |
| `Transform(Func<TSource, TTarget, Task>)`                    | Adds an async transformation                           |
| `Transform(Func<TSource, TTarget, CancellationToken, Task>)` | Adds an async transformation with cancellation support |
```

---

# Implementation Order

1. Update `ITransformationStep` interface to return `Task`
2. Update `PropertyTransformation` to implement async interface
3. Update `TransformTransformation` to support both sync and async delegates
4. Update `AutoMapTransformation` to implement async interface
5. Create `ITransformationStepContainer<TSource, TTarget>` interface
6. Update `TransformationStep` to implement container interface and add async Transform overload
7. Update `IPipeline` with async `Exec`
8. Update `Pipeline` with async implementation and interface-based constructor
9. Update `PipelineBuilder.Build()` to cast to interface
10. Update all unit tests to be async
11. Update README with async examples
12. Build and verify all tests pass

# IPipeline Interface PRD

Add an `IPipeline<TSource, TTarget>` interface as an abstraction over the `Pipeline` class. This interface should be used for dependency injection, enabling better testability and following the Dependency Inversion Principle.

## Overview

- Create `IPipeline<TSource, TTarget>` interface with all public members of `Pipeline`
- Update `Pipeline<TSource, TTarget>` to implement the interface
- Update DI extensions to register pipelines as `IPipeline<TSource, TTarget>`
- Update tests and documentation

---

# IPipeline Interface

## New File: `src/Petl.Core/IPipeline.cs`

- [ ] Create `IPipeline<TSource, TTarget>` interface
- [ ] Include `Exec` method signature
- [ ] Include `StepCount` property
- [ ] Include `StepNames` property

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
    void Exec(TSource source, TTarget target);

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

# Pipeline Class Update

## Update: `src/Petl.Core/Pipeline.cs`

- [ ] Implement `IPipeline<TSource, TTarget>` interface
- [ ] No changes to existing implementation required

```csharp
// Change from:
public class Pipeline<TSource, TTarget>

// To:
public class Pipeline<TSource, TTarget> : IPipeline<TSource, TTarget>
```

---

# PetlBuilderExtensions Updates

## Update: `src/Petl.Core/PetlBuilderExtensions.cs`

- [ ] Update `WithPipeline` methods to register as `IPipeline<TSource, TTarget>`
- [ ] Update `WithAutoMapping` methods to register as `IPipeline<TSource, TTarget>`
- [ ] Keep registering the concrete `Pipeline` instance, but as the interface type

### WithPipeline Changes

```csharp
// Change from:
builder.Services.AddSingleton(pipeline);

// To:
builder.Services.AddSingleton<IPipeline<TSource, TTarget>>(pipeline);

// And for keyed:
// Change from:
builder.Services.AddKeyedSingleton(name, pipeline);

// To:
builder.Services.AddKeyedSingleton<IPipeline<TSource, TTarget>>(name, pipeline);
```

### WithAutoMapping Changes

Same pattern - register as `IPipeline<TSource, TTarget>` instead of concrete type.

---

# Unit Tests

## Update: `src/Petl.Tests/DependencyInjectionTests.cs`

- [ ] Update all `GetService<Pipeline<...>>` to `GetService<IPipeline<...>>`
- [ ] Update all `GetKeyedService<Pipeline<...>>` to `GetKeyedService<IPipeline<...>>`
- [ ] Update all `GetRequiredService<Pipeline<...>>` to `GetRequiredService<IPipeline<...>>`
- [ ] Update all `GetRequiredKeyedService<Pipeline<...>>` to `GetRequiredKeyedService<IPipeline<...>>`

### Example Changes

```csharp
// Change from:
var resolvedPipeline = provider.GetService<Pipeline<DITestSource, DITestTarget>>();

// To:
var resolvedPipeline = provider.GetService<IPipeline<DITestSource, DITestTarget>>();
```

## New Tests: `src/Petl.Tests/PipelineTests.cs`

- [ ] Add test to verify `Pipeline` implements `IPipeline`

```csharp
[TestMethod]
public void Pipeline_ShouldImplementIPipeline()
{
    // Arrange
    var builder = new PipelineBuilder<InputModel, OutputModel>();
    builder.WithStep("Test");
    
    // Act
    var pipeline = builder.Build();
    
    // Assert
    Assert.IsInstanceOfType(pipeline, typeof(IPipeline<InputModel, OutputModel>));
}
```

---

# README Updates

## Update: `README.md`

### Dependency Injection Section

- [ ] Update injection examples to use `IPipeline<,>` interface
- [ ] Emphasize interface-based injection for testability

```csharp
// Update service injection example from:
public class UserService
{
    private readonly Pipeline<UserDto, UserViewModel> _pipeline;

    public UserService(Pipeline<UserDto, UserViewModel> pipeline)
    {
        _pipeline = pipeline;
    }
}

// To:
public class UserService
{
    private readonly IPipeline<UserDto, UserViewModel> _pipeline;

    public UserService(IPipeline<UserDto, UserViewModel> pipeline)
    {
        _pipeline = pipeline;
    }
}
```

### Keyed Services Section

```csharp
// Update from:
public class UserService
{
    public UserService(
        [FromKeyedServices("Summary")] Pipeline<UserDto, UserViewModel> summaryPipeline,
        [FromKeyedServices("Full")] Pipeline<UserDto, UserViewModel> fullPipeline)
    {
    }
}

// To:
public class UserService
{
    public UserService(
        [FromKeyedServices("Summary")] IPipeline<UserDto, UserViewModel> summaryPipeline,
        [FromKeyedServices("Full")] IPipeline<UserDto, UserViewModel> fullPipeline)
    {
    }
}
```

### API Reference Section

- [ ] Add `IPipeline<TSource, TTarget>` to API Reference table

```markdown
### IPipeline<TSource, TTarget>

Interface representing a transformation pipeline. Use this for dependency injection.

| Property/Method | Description |
|-----------------|-------------|
| `Exec(TSource source, TTarget target)` | Executes the transformation pipeline |
| `StepCount` | Gets the number of transformation steps |
| `StepNames` | Gets the names of all transformation steps |
```

---

# Implementation Order

1. Create `IPipeline<TSource, TTarget>` interface
2. Update `Pipeline<TSource, TTarget>` to implement the interface
3. Update `PetlBuilderExtensions` to register as interface type
4. Update `DependencyInjectionTests` to resolve interface type
5. Add interface implementation test to `PipelineTests`
6. Update README with interface-based examples
7. Build and verify all tests pass


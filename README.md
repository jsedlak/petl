<div style="text-align:center;">

![Petl Icon](img/icon.png)

</div>

# Petl

A Programmable ETL (Extract, Transform, Load) Library designed around using a fluent interface to transform data from inputs to outputs.

## Features

- **Fluent Interface**: Easy-to-use builder pattern for creating transformation pipelines
- **Async-First**: All pipeline operations are async with full cancellation token support
- **AutoMap**: Automatically map matching properties between source and target with zero configuration
- **Property Transformations**: Simple type-to-type data copying between properties
- **Custom Transformations**: Support for both sync and async transformation logic
- **Pipeline Steps**: Organize transformations into logical steps
- **Dependency Injection**: First-class support for ASP.NET Core and Microsoft.Extensions.DependencyInjection
- **Type Safety**: Full generic type support for compile-time safety

## Installation

```bash
dotnet add package Petl.Core
```

## Quick Start

### The Simplest Approach: AutoMap

When your source and target types share properties with the same names and types, use `WithAutoMapStep()` for zero-configuration mapping:

```csharp
using Petl;

public class UserDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

public class UserViewModel
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

// One line to map all matching properties!
var pipeline = new PipelineBuilder<UserDto, UserViewModel>()
    .WithAutoMapStep()
    .Build();

var dto = new UserDto { Name = "John", Age = 30, Email = "john@example.com" };
var viewModel = new UserViewModel();

await pipeline.Exec(dto, viewModel);
// viewModel now has all values from dto
```

### AutoMap with Additional Transformations

Combine automatic mapping with custom logic:

```csharp
var pipeline = new PipelineBuilder<UserDto, UserViewModel>()
    .WithAutoMapStep()
    .WithStep("Post-Processing")
        .Transform((source, target) =>
        {
            target.Name = target.Name.ToUpper();
        })
    .Build();

await pipeline.Exec(dto, viewModel);
```

### AutoMap with Filtering

Control which values get copied using a filter callback:

```csharp
var pipeline = new PipelineBuilder<UserDto, UserViewModel>()
    .WithAutoMapStep((source, target, value) =>
    {
        // Only copy non-null, non-empty values
        if (value is string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        return value != null;
    })
    .Build();
```

### Manual Property Mapping

For more control, map properties explicitly:

```csharp
var builder = new PipelineBuilder<InputModel, OutputModel>();

builder
    .WithStep("Transform User Data")
        .Property(x => x.FirstName, y => y.FullName)
        .Property(x => x.BirthYear, y => y.Age)
        .Transform((source, target) =>
        {
            target.DisplayName = $"{source.FirstName} ({source.Age})";
        });

var pipeline = builder.Build();
await pipeline.Exec(input, output);
```

### Async Transformations

The `Transform` method seamlessly handles both sync and async delegates - just write your lambda naturally:

```csharp
var pipeline = new PipelineBuilder<UserDto, UserViewModel>()
    .WithAutoMapStep()
    .WithStep("Process")
        // Sync - just works
        .Transform((source, target) =>
        {
            target.Name = source.Name.Trim();
        })

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

## Dependency Injection

Petl integrates seamlessly with ASP.NET Core and any application using `Microsoft.Extensions.DependencyInjection`.

### Basic Setup

```csharp
// In Program.cs or Startup.cs
builder.Services.AddPetl()
    .WithPipeline<UserDto, UserViewModel>(pipeline =>
    {
        pipeline.WithAutoMapStep();
    });
```

### Inject and Use

Pipelines are registered as `IPipeline<TSource, TTarget>` for better testability and following the Dependency Inversion Principle:

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

### AutoMapping with DI

Register auto-mapped pipelines with a single line:

```csharp
builder.Services.AddPetl()
    .WithAutoMapping<UserDto, UserViewModel>();
```

Or with additional configuration:

```csharp
builder.Services.AddPetl()
    .WithAutoMapping<UserDto, UserViewModel>(pipeline =>
    {
        pipeline.WithStep("Normalize")
            .Transform((source, target) =>
            {
                target.Email = target.Email.ToLower();
            });
    });
```

### Named/Keyed Services

Register multiple pipelines for the same types using keyed services:

```csharp
builder.Services.AddPetl()
    .WithPipeline<UserDto, UserViewModel>("Summary", pipeline =>
    {
        pipeline.WithStep("Summary")
            .Property(x => x.Name, y => y.Name);
    })
    .WithPipeline<UserDto, UserViewModel>("Full", pipeline =>
    {
        pipeline.WithAutoMapStep();
    });
```

Inject with `[FromKeyedServices]`:

```csharp
public class UserService
{
    public UserService(
        [FromKeyedServices("Summary")] IPipeline<UserDto, UserViewModel> summaryPipeline,
        [FromKeyedServices("Full")] IPipeline<UserDto, UserViewModel> fullPipeline)
    {
        // Use different pipelines for different scenarios
    }
}
```

### Full DI Options

```csharp
builder.Services.AddPetl()
    // Simple auto-mapping
    .WithAutoMapping<SourceA, TargetA>()
    
    // Auto-mapping with filter
    .WithAutoMapping<SourceB, TargetB>((src, tgt, val) => val != null)
    
    // Auto-mapping with additional steps
    .WithAutoMapping<SourceC, TargetC>(pipeline =>
    {
        pipeline.WithStep("Extra").Transform((s, t) =>
        {
            t.Processed = true;
        });
    })
    
    // Keyed auto-mapping with filter and additional steps
    .WithAutoMapping<SourceD, TargetD>(
        name: "CustomMapper",
        filter: (src, tgt, val) => val != null,
        configure: pipeline =>
        {
            pipeline.WithStep("Finalize")
                .Transform((s, t) =>
                {
                    t.Name = t.Name.Trim();
                });
        })
    
    // Custom pipeline
    .WithPipeline<SourceE, TargetE>(pipeline =>
    {
        pipeline.WithStep("Manual")
            .Property(x => x.Id, y => y.Identifier)
            .Property(x => x.Value, y => y.Amount);
    });
```

## API Reference

### PipelineBuilder<TSource, TTarget>

The main entry point for creating transformation pipelines.

| Method | Description |
|--------|-------------|
| `WithStep(string stepName)` | Creates a new transformation step |
| `WithAutoMapStep(string stepName = "AutoMap")` | Auto-maps all matching properties |
| `WithAutoMapStep(Func<...> filter, string stepName = "AutoMap")` | Auto-maps with a filter callback |
| `Build()` | Builds the pipeline |

### TransformationStep<TSource, TTarget>

Represents a single step in the transformation pipeline.

| Method | Description |
|--------|-------------|
| `Property(...)` | Maps a source property to a target property |
| `Transform(Action<TSource, TTarget>)` | Adds a sync transformation |
| `Transform(Func<TSource, TTarget, Task>)` | Adds an async transformation |
| `Transform(Func<TSource, TTarget, CancellationToken, Task>)` | Adds an async transformation with cancellation support |

### IPipeline<TSource, TTarget>

Interface representing a transformation pipeline. Use this for dependency injection.

| Property/Method | Description |
|-----------------|-------------|
| `Task Exec(TSource, TTarget, CancellationToken)` | Executes the transformation pipeline |
| `StepCount` | Gets the number of transformation steps |
| `StepNames` | Gets the names of all transformation steps |

### Pipeline<TSource, TTarget>

The concrete implementation of `IPipeline<TSource, TTarget>`.

| Property/Method | Description |
|-----------------|-------------|
| `Task Exec(TSource, TTarget, CancellationToken)` | Executes the transformation pipeline |
| `StepCount` | Gets the number of transformation steps |
| `StepNames` | Gets the names of all transformation steps |

### ServiceCollectionExtensions

Extension methods for dependency injection.

| Method | Description |
|--------|-------------|
| `AddPetl()` | Adds Petl support, returns `PetlBuilder` |

### PetlBuilderExtensions

Methods for registering pipelines with DI.

| Method | Description |
|--------|-------------|
| `WithPipeline<TSource, TTarget>(configure)` | Registers a custom pipeline |
| `WithPipeline<TSource, TTarget>(name, configure)` | Registers a keyed custom pipeline |
| `WithAutoMapping<TSource, TTarget>(...)` | Registers an auto-mapped pipeline (multiple overloads) |

## License

This project is part of the Petl ETL library.

# Dependency Injection Extensions PRD

Add service collection extensions to support registering Petl pipelines as injectable services. This enables pipelines to be resolved through dependency injection in ASP.NET Core and other Microsoft.Extensions.DependencyInjection-based applications.

## Overview

The extensions should provide:

- A fluent API starting with `AddPetl()` on `IServiceCollection`
- `WithPipeline<TInput, TOutput>()` method for registering custom pipelines
- `WithAutoMapping<TInput, TOutput>()` method for registering auto-mapped pipelines
- Support for both regular and keyed service registration

---

# Petl.Core Project Updates

## Package Reference

- [ ] Add package reference to `Microsoft.Extensions.DependencyInjection.Abstractions` in `Petl.Core.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
</ItemGroup>
```

---

# PetlBuilder Class

## New File: `src/Petl.Core/PetlBuilder.cs`

- [ ] Create `PetlBuilder` class to hold `IServiceCollection` reference
- [ ] Implement fluent API for pipeline registration

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Petl;

public class PetlBuilder
{
    public IServiceCollection Services { get; }

    public PetlBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
```

---

# ServiceCollectionExtensions Class

## New File: `src/Petl.Core/ServiceCollectionExtensions.cs`

### `AddPetl` Method

- [ ] Add `AddPetl()` extension method on `IServiceCollection`
- [ ] Returns `PetlBuilder` for fluent chaining

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Petl;

public static class ServiceCollectionExtensions
{
    public static PetlBuilder AddPetl(this IServiceCollection services)
    {
        return new PetlBuilder(services);
    }
}
```

---

# PetlBuilderExtensions Class

## New File: `src/Petl.Core/PetlBuilderExtensions.cs`

### `WithPipeline` Method (No Name - Regular Service)

- [ ] Add `WithPipeline<TSource, TTarget>(Action<PipelineBuilder<TSource, TTarget>> configure)` extension
- [ ] Registers `Pipeline<TSource, TTarget>` as a regular singleton service
- [ ] Returns `PetlBuilder` for chaining

```csharp
// Usage:
builder.Services.AddPetl()
    .WithPipeline<InputModel, OutputModel>(pipeline =>
    {
        pipeline.WithStep("Step1")
            .Property(x => x.Name, y => y.FullName);
    });

// Injection:
public class MyService(Pipeline<InputModel, OutputModel> pipeline) { }
```

### `WithPipeline` Method (With Name - Keyed Service)

- [ ] Add `WithPipeline<TSource, TTarget>(string name, Action<PipelineBuilder<TSource, TTarget>> configure)` overload
- [ ] Registers `Pipeline<TSource, TTarget>` as a keyed singleton service using the name
- [ ] Returns `PetlBuilder` for chaining

```csharp
// Usage:
builder.Services.AddPetl()
    .WithPipeline<InputModel, OutputModel>("UserMapping", pipeline =>
    {
        pipeline.WithStep("Step1")
            .Property(x => x.Name, y => y.FullName);
    });

// Injection with [FromKeyedServices]:
public class MyService([FromKeyedServices("UserMapping")] Pipeline<InputModel, OutputModel> pipeline) { }
```

### `WithAutoMapping` Method (No Name - Regular Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>()` extension
- [ ] Internally calls `WithAutoMapStep()` on a new `PipelineBuilder`
- [ ] Registers `Pipeline<TSource, TTarget>` as a regular singleton service
- [ ] Returns `PetlBuilder` for chaining

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>();
```

### `WithAutoMapping` Method (With Configure Callback - Regular Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(Action<PipelineBuilder<TSource, TTarget>> configure)` overload
- [ ] Calls `WithAutoMapStep()` first, then invokes the configure callback
- [ ] Allows adding additional steps after auto-mapping
- [ ] Registers as regular singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>(pipeline =>
    {
        pipeline.WithStep("PostProcessing")
            .Transform((source, target) => target.Name = target.Name.ToUpper());
    });
```

### `WithAutoMapping` Method (With Name - Keyed Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(string name)` overload
- [ ] Registers as keyed singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>("AutoUserMapping");
```

### `WithAutoMapping` Method (With Name and Configure Callback - Keyed Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(string name, Action<PipelineBuilder<TSource, TTarget>> configure)` overload
- [ ] Calls `WithAutoMapStep()` first, then invokes the configure callback
- [ ] Registers as keyed singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>("CustomMapping", pipeline =>
    {
        pipeline.WithStep("PostProcessing")
            .Transform((source, target) => target.Name = target.Name.ToUpper());
    });
```

### `WithAutoMapping` Method (With Filter - Regular Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(Func<TSource, TTarget, object?, bool> filter)` overload
- [ ] Internally calls `WithAutoMapStep(filter)` on a new `PipelineBuilder`
- [ ] Registers as regular singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>((source, target, value) => value != null);
```

### `WithAutoMapping` Method (With Filter and Configure Callback - Regular Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(Func<TSource, TTarget, object?, bool> filter, Action<PipelineBuilder<TSource, TTarget>> configure)` overload
- [ ] Calls `WithAutoMapStep(filter)` first, then invokes the configure callback
- [ ] Registers as regular singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>(
        (source, target, value) => value != null,
        pipeline =>
        {
            pipeline.WithStep("PostProcessing")
                .Transform((source, target) => target.Name = target.Name.ToUpper());
        });
```

### `WithAutoMapping` Method (With Name and Filter - Keyed Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(string name, Func<TSource, TTarget, object?, bool> filter)` overload
- [ ] Registers as keyed singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>("FilteredMapping", (source, target, value) => value != null);
```

### `WithAutoMapping` Method (Full Options - Keyed Service)

- [ ] Add `WithAutoMapping<TSource, TTarget>(string name, Func<TSource, TTarget, object?, bool>? filter, Action<PipelineBuilder<TSource, TTarget>>? configure)` overload
- [ ] Core implementation that all other overloads delegate to
- [ ] Registers as keyed singleton service

```csharp
// Usage:
builder.Services.AddPetl()
    .WithAutoMapping<InputModel, OutputModel>(
        "FullMapping",
        (source, target, value) => value != null,
        pipeline =>
        {
            pipeline.WithStep("PostProcessing")
                .Transform((source, target) => target.Name = target.Name.ToUpper());
        });
```

## Implementation Details

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Petl;

public static class PetlBuilderExtensions
{
    // WithPipeline - regular service
    public static PetlBuilder WithPipeline<TSource, TTarget>(
        this PetlBuilder builder,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        configure(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddSingleton(pipeline);
        return builder;
    }

    // WithPipeline - keyed service
    public static PetlBuilder WithPipeline<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        configure(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddKeyedSingleton(name, pipeline);
        return builder;
    }

    // WithAutoMapping - regular service (minimal)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder)
    {
        return WithAutoMapping<TSource, TTarget>(builder, filter: null, configure: null);
    }

    // WithAutoMapping - regular service (with configure)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        return WithAutoMapping<TSource, TTarget>(builder, filter: null, configure: configure);
    }

    // WithAutoMapping - regular service (with filter)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        Func<TSource, TTarget, object?, bool> filter)
    {
        return WithAutoMapping<TSource, TTarget>(builder, filter: filter, configure: null);
    }

    // WithAutoMapping - regular service (with filter and configure)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        Func<TSource, TTarget, object?, bool>? filter,
        Action<PipelineBuilder<TSource, TTarget>>? configure)
    {
        var pipelineBuilder = new PipelineBuilder<TSource, TTarget>();
        pipelineBuilder.WithAutoMapStep(filter);
        configure?.Invoke(pipelineBuilder);
        var pipeline = pipelineBuilder.Build();

        builder.Services.AddSingleton(pipeline);
        return builder;
    }

    // WithAutoMapping - keyed service (name only)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name)
    {
        return WithAutoMapping<TSource, TTarget>(builder, name, filter: null, configure: null);
    }

    // WithAutoMapping - keyed service (with configure)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Action<PipelineBuilder<TSource, TTarget>> configure)
    {
        return WithAutoMapping<TSource, TTarget>(builder, name, filter: null, configure: configure);
    }

    // WithAutoMapping - keyed service (with filter)
    public static PetlBuilder WithAutoMapping<TSource, TTarget>(
        this PetlBuilder builder,
        string name,
        Func<TSource, TTarget, object?, bool> filter)
    {
        return WithAutoMapping<TSource, TTarget>(builder, name, filter: filter, configure: null);
    }

    // WithAutoMapping - keyed service (full options - core implementation)
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

        builder.Services.AddKeyedSingleton(name, pipeline);
        return builder;
    }
}
```

---

# Unit Tests

## Updates to: `src/Petl.Tests/Petl.Tests.csproj`

- [ ] Add package reference to `Microsoft.Extensions.DependencyInjection` (full package for testing)

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
</ItemGroup>
```

## New Test File: `src/Petl.Tests/DependencyInjectionTests.cs`

### ServiceCollection Extension Tests

- [ ] `AddPetl_ShouldReturnPetlBuilder` - verify builder is returned
- [ ] `AddPetl_ShouldContainServiceCollection` - verify services accessible

### WithPipeline Tests

- [ ] `WithPipeline_ShouldRegisterPipeline` - verify pipeline is registered
- [ ] `WithPipeline_ShouldExecuteConfigureAction` - verify configuration is applied
- [ ] `WithPipeline_ShouldReturnBuilderForChaining` - verify fluent API
- [ ] `WithPipeline_WithName_ShouldRegisterKeyedService` - verify keyed registration
- [ ] `WithPipeline_MultiplePipelines_ShouldRegisterAll` - verify multiple registrations

### WithAutoMapping Tests

- [ ] `WithAutoMapping_ShouldRegisterAutoMappedPipeline` - verify auto-map pipeline registered
- [ ] `WithAutoMapping_WithName_ShouldRegisterKeyedService` - verify keyed registration
- [ ] `WithAutoMapping_WithFilter_ShouldApplyFilter` - verify filter is used
- [ ] `WithAutoMapping_WithNameAndFilter_ShouldWork` - verify both name and filter
- [ ] `WithAutoMapping_WithConfigure_ShouldAddAdditionalSteps` - verify configure callback adds steps
- [ ] `WithAutoMapping_WithNameAndConfigure_ShouldWork` - verify keyed with configure
- [ ] `WithAutoMapping_WithFilterAndConfigure_ShouldWork` - verify filter and configure together
- [ ] `WithAutoMapping_WithAllOptions_ShouldWork` - verify name, filter, and configure together

### Integration Tests

- [ ] `Pipeline_ShouldBeResolvable_FromServiceProvider` - verify DI resolution
- [ ] `KeyedPipeline_ShouldBeResolvable_WithKey` - verify keyed DI resolution

### Test Models

```csharp
public class DITestSource
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class DITestTarget
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

---

# Implementation Order

1. Add `Microsoft.Extensions.DependencyInjection.Abstractions` package reference to `Petl.Core`
2. Create `PetlBuilder` class in `Petl.Core`
3. Create `ServiceCollectionExtensions` with `AddPetl()` method
4. Create `PetlBuilderExtensions` with `WithPipeline` methods (both overloads)
5. Add `WithAutoMapping` methods (all overloads)
6. Add `Microsoft.Extensions.DependencyInjection` package reference to `Petl.Tests`
7. Create `DependencyInjectionTests.cs` with unit tests
8. Verify all tests pass

# AutoMap Feature PRD

Add the ability to automatically map properties from the source object to the target object via a single TransformationStep. This feature eliminates the need to manually specify each property mapping when source and target objects share properties with matching names and types.

## Overview

The `PipelineBuilder` should support a new method `WithAutoMapStep()` that:

- Compares source and target types using reflection
- Identifies properties with matching names and compatible types
- Creates an `AutoMapTransformation` step that copies all matched property values
- Supports an optional filter callback for conditional value copying

---

# PipelineBuilder Changes

## `WithAutoMapStep` Method

- [ ] Add `WithAutoMapStep(string stepName = "AutoMap")` method to `PipelineBuilder<TSource, TTarget>`
- [ ] Method should return `PipelineBuilder<TSource, TTarget>` for method chaining
- [ ] Default step name should be `"AutoMap"` when not provided

```csharp
// Usage examples:
builder.WithAutoMapStep(); // Uses default name "AutoMap"
builder.WithAutoMapStep("CustomStepName"); // Uses provided name
```

## `WithAutoMapStep` Overload with Filter Callback

- [ ] Add overload: `WithAutoMapStep(Func<TSource, TTarget, object?, bool> filter, string stepName = "AutoMap")`
- [ ] The filter callback receives:
  - `TSource source` - the source object
  - `TTarget target` - the target object
  - `object? value` - the value being copied
- [ ] Filter returns `true` to copy the value, `false` to skip
- [ ] When filter is not provided (null or first overload), all matching values are copied

```csharp
// Usage with filter:
builder.WithAutoMapStep((source, target, value) =>
{
    // Only copy non-null values
    return value != null;
});

// With custom name:
builder.WithAutoMapStep((source, target, value) => value != null, "FilteredAutoMap");
```

## Property Matching Logic

- [ ] Use reflection to get public instance properties from both `TSource` and `TTarget`
- [ ] Match properties by **exact name** (case-sensitive)
- [ ] Match properties by **compatible type**:
  - Exact type match
  - Target type is assignable from source type
  - Nullable to/from non-nullable of same underlying type
- [ ] Source properties must be readable (`CanRead`)
- [ ] Target properties must be writable (`CanWrite`)

```csharp
// Reference file: src/Petl.Core/PipelineBuilder.cs

private static bool AreTypesCompatible(Type sourceType, Type targetType)
{
    // Exact match
    if (sourceType == targetType)
        return true;

    // Target is assignable from source
    if (targetType.IsAssignableFrom(sourceType))
        return true;

    // Handle nullable conversions
    var underlyingSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
    var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

    return underlyingSource == underlyingTarget;
}
```

---

# AutoMapTransformation Class

## New File: `src/Petl.Core/AutoMapTransformation.cs`

- [ ] Create internal class `AutoMapTransformation<TSource, TTarget>` implementing `ITransformationStep`
- [ ] Store list of matched property pairs (`PropertyInfo` source and target)
- [ ] Store optional filter callback
- [ ] Implement `Execute(object source, object target)` method

## Constructor

- [ ] Accept `IEnumerable<(PropertyInfo Source, PropertyInfo Target)>` for matched properties
- [ ] Accept optional `Func<TSource, TTarget, object?, bool>? filter` parameter

## Execute Method

- [ ] Iterate through matched property pairs
- [ ] For each pair:
  - Get value from source property
  - If filter is provided, call filter and skip if returns false
  - Handle null values appropriately (only set if target allows null)
  - Set value on target property

```csharp
public void Execute(object source, object target)
{
    if (source is not TSource typedSource || target is not TTarget typedTarget)
        return;

    foreach (var (sourceProp, targetProp) in _propertyMappings)
    {
        var value = sourceProp.GetValue(typedSource);

        // Apply filter if provided
        if (_filter != null && !_filter(typedSource, typedTarget, value))
            continue;

        // Copy value to target
        SetTargetValue(typedTarget, targetProp, value);
    }
}
```

---

# TransformationStep Integration

## Reference: `src/Petl.Core/TransformationStep.cs`

- [ ] No changes needed to `TransformationStep` class
- [ ] `AutoMapTransformation` implements `ITransformationStep` directly
- [ ] Added via `PipelineBuilder`, not through `TransformationStep`

---

# Unit Tests

## New Tests in `src/Petl.Tests/PipelineTests.cs`

### Basic AutoMap Tests

- [ ] `AutoMap_ShouldMapMatchingProperties` - verify all matching properties are copied
- [ ] `AutoMap_ShouldOnlyMapMatchingProperties` - verify non-matching properties are unchanged
- [ ] `AutoMap_ShouldNotMapTypeMismatchedProperties` - verify type mismatches are skipped
- [ ] `AutoMap_WithNoMatchingProperties_ShouldNotChangeTarget` - verify no-match scenario

### Nullable Type Tests

- [ ] `AutoMap_ShouldMapNullableProperties` - nullable to nullable mapping
- [ ] `AutoMap_ShouldHandleNullValues` - null value handling
- [ ] `AutoMap_ShouldMapNullableToNonNullable` - int? to int when value present

### Step Configuration Tests

- [ ] `AutoMap_ShouldCreateStepWithDefaultName` - verify default "AutoMap" name
- [ ] `AutoMap_ShouldCreateStepWithCustomName` - verify custom name works
- [ ] `AutoMap_ShouldReturnBuilderForChaining` - verify fluent API
- [ ] `AutoMap_ShouldWorkWithOtherSteps` - verify works in pipeline with other steps

### Filter Callback Tests

- [ ] `AutoMap_WithFilter_ShouldApplyFilter` - verify filter is called
- [ ] `AutoMap_WithFilter_ShouldSkipWhenFilterReturnsFalse` - verify skip behavior
- [ ] `AutoMap_WithFilter_ShouldCopyWhenFilterReturnsTrue` - verify copy behavior
- [ ] `AutoMap_WithFilter_ShouldReceiveCorrectParameters` - verify source, target, value passed correctly

### Test Models Needed

```csharp
// Models for testing
public class AutoMapSource
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
}

public class AutoMapTarget
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
}

public class PartialMatchTarget
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Address { get; set; } // No match
}

public class TypeMismatchTarget
{
    public string Name { get; set; }
    public string Age { get; set; } // int vs string mismatch
}

public class NullableSource
{
    public int? NullableInt { get; set; }
    public string? NullableString { get; set; }
}

public class NullableTarget
{
    public int? NullableInt { get; set; }
    public string? NullableString { get; set; }
}
```

---

# Implementation Order

1. Create `AutoMapTransformation<TSource, TTarget>` class
2. Add `WithAutoMapStep(string stepName = "AutoMap")` to `PipelineBuilder`
3. Add `WithAutoMapStep(Func<...> filter, string stepName = "AutoMap")` overload
4. Add unit tests for basic functionality
5. Add unit tests for filter functionality
6. Verify all existing tests still pass

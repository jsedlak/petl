Petl is a Programmable ETL (Extract, Transform, Load) Library designed around using a fluent interface to transform data from inputs to outputs.

Each pipeline is broken down into a number of steps, each containing a set of transformations. Built-in transformations consist of a Property transformation (type for type data copy) as well as a Transform which supports calling back to a handler that takes in the input and output objects.

```csharp
var builder = new PipelineBuilder<InputModel, OutputModel>();

builder
    .WithStep("Simple Transform")
        .Property(
            x => x.SourceProperty,
            y => y.TargetProperty
        )
        .Transform((source, target) => {
            target.SomeProperty = source.SomeProperty.ToString();
        })

var pipeline = builder.Build();

var myInput = new InputObject();
var myOutput = new OutputObject();

pipeline.Exec(myInput, myOutput);
```

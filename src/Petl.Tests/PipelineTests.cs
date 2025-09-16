using Microsoft.VisualStudio.TestTools.UnitTesting;
using Petl;

namespace Petl.Tests;

/// <summary>
/// Unit tests for the Petl.Core library
/// </summary>
[TestClass]
public class PipelineTests
{
    /// <summary>
    /// Example input model for testing
    /// </summary>
    public class InputModel
    {
        public string SourceProperty { get; set; } = string.Empty;
        public int SomeProperty { get; set; }
    }

    /// <summary>
    /// Example output model for testing
    /// </summary>
    public class OutputModel
    {
        public string TargetProperty { get; set; } = string.Empty;
        public string SomeProperty { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test input model with additional properties
    /// </summary>
    public class TestInput
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// Test output model with additional properties
    /// </summary>
    public class TestOutput
    {
        public string FullName { get; set; } = string.Empty;
        public int YearsOld { get; set; }
        public string BirthYear { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [TestMethod]
    public void PipelineBuilder_WithStep_ShouldCreateStep()
    {
        // Arrange
        var builder = new PipelineBuilder<InputModel, OutputModel>();

        // Act
        var step = builder.WithStep("Test Step");

        // Assert
        Assert.IsNotNull(step);
        Assert.AreEqual("Test Step", step.StepName);
    }

    [TestMethod]
    public void PipelineBuilder_Build_ShouldCreatePipeline()
    {
        // Arrange
        var builder = new PipelineBuilder<InputModel, OutputModel>();
        builder.WithStep("Test Step");

        // Act
        var pipeline = builder.Build();

        // Assert
        Assert.IsNotNull(pipeline);
        Assert.AreEqual(1, pipeline.StepCount);
    }

    [TestMethod]
    public void Pipeline_Exec_ShouldExecutePropertyTransformation()
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
        pipeline.Exec(input, output);

        // Assert
        Assert.AreEqual("Hello World", output.TargetProperty);
    }

    [TestMethod]
    public void Pipeline_Exec_ShouldExecuteCustomTransform()
    {
        // Arrange
        var builder = new PipelineBuilder<InputModel, OutputModel>();
        builder
            .WithStep("Custom Transform")
                .Transform((source, target) => {
                    target.SomeProperty = source.SomeProperty.ToString();
                });

        var pipeline = builder.Build();
        var input = new InputModel { SomeProperty = 42 };
        var output = new OutputModel();

        // Act
        pipeline.Exec(input, output);

        // Assert
        Assert.AreEqual("42", output.SomeProperty);
    }

    [TestMethod]
    public void Pipeline_Exec_ShouldExecuteMultipleSteps()
    {
        // Arrange
        var builder = new PipelineBuilder<TestInput, TestOutput>();
        
        var step1 = builder
            .WithStep("Basic Property Mapping")
                .Property(x => x.Name, y => y.FullName)
                .Property(x => x.Age, y => y.YearsOld);

        var step2 = builder
            .WithStep("Custom Transformations")
                .Transform((source, target) => {
                    target.BirthYear = source.BirthDate.Year.ToString();
                })
                .Transform((source, target) => {
                    target.Description = $"{source.Name} is {source.Age} years old, born in {source.BirthDate.Year}";
                });

        var pipeline = builder.Build();
        var input = new TestInput
        {
            Name = "John Doe",
            Age = 30,
            BirthDate = new DateTime(1993, 5, 15)
        };
        var output = new TestOutput();

        // Act
        pipeline.Exec(input, output);

        // Assert
        Assert.AreEqual("John Doe", output.FullName);
        Assert.AreEqual(30, output.YearsOld);
        Assert.AreEqual("1993", output.BirthYear);
        Assert.AreEqual("John Doe is 30 years old, born in 1993", output.Description);
        Assert.AreEqual(2, pipeline.StepCount);
    }

    [TestMethod]
    public void Pipeline_Exec_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new PipelineBuilder<InputModel, OutputModel>();
        builder.WithStep("Test Step");
        var pipeline = builder.Build();
        var output = new OutputModel();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => pipeline.Exec(null!, output));
    }

    [TestMethod]
    public void Pipeline_Exec_WithNullTarget_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new PipelineBuilder<InputModel, OutputModel>();
        builder.WithStep("Test Step");
        var pipeline = builder.Build();
        var input = new InputModel();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => pipeline.Exec(input, null!));
    }

    [TestMethod]
    public void Pipeline_StepNames_ShouldReturnCorrectNames()
    {
        // Arrange
        var builder = new PipelineBuilder<InputModel, OutputModel>();
        builder.WithStep("Step 1");
        builder.WithStep("Step 2");
        var pipeline = builder.Build();

        // Act
        var stepNames = pipeline.StepNames.ToList();

        // Assert
        Assert.AreEqual(2, stepNames.Count);
        Assert.IsTrue(stepNames.Contains("Step 1"));
        Assert.IsTrue(stepNames.Contains("Step 2"));
    }
}

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

    #region AutoMap Tests

    /// <summary>
    /// Source model for AutoMap testing with matching properties
    /// </summary>
    public class AutoMapSource
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }

    /// <summary>
    /// Target model for AutoMap testing with matching properties
    /// </summary>
    public class AutoMapTarget
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal Salary { get; set; }
    }

    /// <summary>
    /// Target model with only some matching properties
    /// </summary>
    public class PartialMatchTarget
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Address { get; set; } = string.Empty; // No match in source
    }

    /// <summary>
    /// Target model with type mismatches
    /// </summary>
    public class TypeMismatchTarget
    {
        public string Name { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty; // Type mismatch: int vs string
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// Source model with nullable properties
    /// </summary>
    public class NullableSource
    {
        public int? NullableInt { get; set; }
        public string? NullableString { get; set; }
        public DateTime? NullableDate { get; set; }
        public int RegularInt { get; set; }
    }

    /// <summary>
    /// Target model with nullable properties
    /// </summary>
    public class NullableTarget
    {
        public int? NullableInt { get; set; }
        public string? NullableString { get; set; }
        public DateTime? NullableDate { get; set; }
        public int RegularInt { get; set; }
    }

    /// <summary>
    /// Target model with non-nullable matching nullable source
    /// </summary>
    public class NullableToNonNullableTarget
    {
        public int NullableInt { get; set; } // Source is int?, target is int
    }

    /// <summary>
    /// Model with no matching properties in target
    /// </summary>
    public class NoMatchTarget
    {
        public string FirstName { get; set; } = string.Empty;
        public int YearsOld { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
    }

    [TestMethod]
    public void AutoMap_ShouldMapMatchingProperties()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "John Doe",
            Age = 30,
            BirthDate = new DateTime(1993, 5, 15),
            Email = "john@example.com",
            Salary = 50000.00m
        };
        var target = new AutoMapTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual("John Doe", target.Name);
        Assert.AreEqual(30, target.Age);
        Assert.AreEqual(new DateTime(1993, 5, 15), target.BirthDate);
        Assert.AreEqual("john@example.com", target.Email);
        Assert.AreEqual(50000.00m, target.Salary);
    }

    [TestMethod]
    public void AutoMap_ShouldOnlyMapMatchingProperties()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, PartialMatchTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "Jane Doe",
            Age = 25,
            BirthDate = new DateTime(1998, 3, 20),
            Email = "jane@example.com",
            Salary = 60000.00m
        };
        var target = new PartialMatchTarget { Address = "Original Address" };

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual("Jane Doe", target.Name);
        Assert.AreEqual(25, target.Age);
        Assert.AreEqual("Original Address", target.Address); // Should remain unchanged
    }

    [TestMethod]
    public void AutoMap_ShouldNotMapTypeMismatchedProperties()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, TypeMismatchTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "Bob Smith",
            Age = 40,
            BirthDate = new DateTime(1983, 7, 10)
        };
        var target = new TypeMismatchTarget { Age = "Original Age" };

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual("Bob Smith", target.Name);
        Assert.AreEqual("Original Age", target.Age); // Should remain unchanged (type mismatch)
        Assert.AreEqual(new DateTime(1983, 7, 10), target.BirthDate);
    }

    [TestMethod]
    public void AutoMap_ShouldMapNullableProperties()
    {
        // Arrange
        var builder = new PipelineBuilder<NullableSource, NullableTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new NullableSource
        {
            NullableInt = 42,
            NullableString = "Hello",
            NullableDate = new DateTime(2024, 1, 1),
            RegularInt = 100
        };
        var target = new NullableTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual(42, target.NullableInt);
        Assert.AreEqual("Hello", target.NullableString);
        Assert.AreEqual(new DateTime(2024, 1, 1), target.NullableDate);
        Assert.AreEqual(100, target.RegularInt);
    }

    [TestMethod]
    public void AutoMap_ShouldHandleNullValues()
    {
        // Arrange
        var builder = new PipelineBuilder<NullableSource, NullableTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new NullableSource
        {
            NullableInt = null,
            NullableString = null,
            NullableDate = null,
            RegularInt = 50
        };
        var target = new NullableTarget
        {
            NullableInt = 999,
            NullableString = "Original",
            NullableDate = DateTime.Now
        };

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.IsNull(target.NullableInt);
        Assert.IsNull(target.NullableString);
        Assert.IsNull(target.NullableDate);
        Assert.AreEqual(50, target.RegularInt);
    }

    [TestMethod]
    public void AutoMap_ShouldMapNullableToNonNullable()
    {
        // Arrange
        var builder = new PipelineBuilder<NullableSource, NullableToNonNullableTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new NullableSource
        {
            NullableInt = 42
        };
        var target = new NullableToNonNullableTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual(42, target.NullableInt);
    }

    [TestMethod]
    public void AutoMap_ShouldCreateStepWithDefaultName()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();

        // Act
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        // Assert
        Assert.AreEqual(1, pipeline.StepCount);
        Assert.IsTrue(pipeline.StepNames.Contains("AutoMap"));
    }

    [TestMethod]
    public void AutoMap_ShouldCreateStepWithCustomName()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();

        // Act
        builder.WithAutoMapStep("CustomAutoMap");
        var pipeline = builder.Build();

        // Assert
        Assert.AreEqual(1, pipeline.StepCount);
        Assert.IsTrue(pipeline.StepNames.Contains("CustomAutoMap"));
    }

    [TestMethod]
    public void AutoMap_ShouldWorkWithOtherSteps()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep();
        builder.WithStep("Custom Step")
            .Transform((source, target) =>
            {
                target.Name = target.Name.ToUpper();
            });

        var pipeline = builder.Build();
        var source = new AutoMapSource { Name = "john doe", Age = 30 };
        var target = new AutoMapTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual("JOHN DOE", target.Name); // Modified by custom step
        Assert.AreEqual(30, target.Age); // Mapped by AutoMap
        Assert.AreEqual(2, pipeline.StepCount);
    }

    [TestMethod]
    public void AutoMap_ShouldReturnBuilderForChaining()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();

        // Act
        var result = builder.WithAutoMapStep();

        // Assert
        Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void AutoMap_WithNoMatchingProperties_ShouldNotChangeTarget()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, NoMatchTarget>();
        builder.WithAutoMapStep();
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "Test",
            Age = 25,
            Email = "test@example.com"
        };
        var target = new NoMatchTarget
        {
            FirstName = "Original",
            YearsOld = 100,
            EmailAddress = "original@example.com"
        };

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual("Original", target.FirstName);
        Assert.AreEqual(100, target.YearsOld);
        Assert.AreEqual("original@example.com", target.EmailAddress);
    }

    [TestMethod]
    public void AutoMap_WithFilter_ShouldApplyFilter()
    {
        // Arrange
        var filterCalled = false;
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep((source, target, value) =>
        {
            filterCalled = true;
            return true;
        });
        var pipeline = builder.Build();

        var source = new AutoMapSource { Name = "Test" };
        var target = new AutoMapTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.IsTrue(filterCalled);
    }

    [TestMethod]
    public void AutoMap_WithFilter_ShouldSkipWhenFilterReturnsFalse()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep((source, target, value) =>
        {
            // Skip all values
            return false;
        });
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "Test",
            Age = 25,
            Email = "test@example.com"
        };
        var target = new AutoMapTarget
        {
            Name = "Original",
            Age = 100,
            Email = "original@example.com"
        };

        // Act
        pipeline.Exec(source, target);

        // Assert - all values should remain unchanged
        Assert.AreEqual("Original", target.Name);
        Assert.AreEqual(100, target.Age);
        Assert.AreEqual("original@example.com", target.Email);
    }

    [TestMethod]
    public void AutoMap_WithFilter_ShouldCopyWhenFilterReturnsTrue()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep((source, target, value) =>
        {
            // Only copy non-null values
            return value != null;
        });
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "Test",
            Age = 25
        };
        var target = new AutoMapTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.AreEqual("Test", target.Name);
        Assert.AreEqual(25, target.Age);
    }

    [TestMethod]
    public void AutoMap_WithFilter_ShouldReceiveCorrectParameters()
    {
        // Arrange
        AutoMapSource? capturedSource = null;
        AutoMapTarget? capturedTarget = null;
        var capturedValues = new List<object?>();

        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep((source, target, value) =>
        {
            capturedSource = source;
            capturedTarget = target;
            capturedValues.Add(value);
            return true;
        });
        var pipeline = builder.Build();

        var source = new AutoMapSource
        {
            Name = "Test",
            Age = 25
        };
        var target = new AutoMapTarget();

        // Act
        pipeline.Exec(source, target);

        // Assert
        Assert.IsNotNull(capturedSource);
        Assert.IsNotNull(capturedTarget);
        Assert.AreSame(source, capturedSource);
        Assert.AreSame(target, capturedTarget);
        Assert.IsTrue(capturedValues.Count > 0);
        Assert.IsTrue(capturedValues.Contains("Test"));
        Assert.IsTrue(capturedValues.Contains(25));
    }

    [TestMethod]
    public void AutoMap_WithFilter_AndCustomName_ShouldWork()
    {
        // Arrange
        var builder = new PipelineBuilder<AutoMapSource, AutoMapTarget>();
        builder.WithAutoMapStep((source, target, value) => value != null, "FilteredAutoMap");
        var pipeline = builder.Build();

        // Assert
        Assert.AreEqual(1, pipeline.StepCount);
        Assert.IsTrue(pipeline.StepNames.Contains("FilteredAutoMap"));
    }

    #endregion
}

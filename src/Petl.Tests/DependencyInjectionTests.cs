using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Petl;

namespace Petl.Tests;

/// <summary>
/// Unit tests for dependency injection extensions
/// </summary>
[TestClass]
public class DependencyInjectionTests
{
    #region Test Models

    public class DITestSource
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class DITestTarget
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class DifferentTarget
    {
        public string FullName { get; set; } = string.Empty;
        public int YearsOld { get; set; }
    }

    #endregion

    #region AddPetl Tests

    [TestMethod]
    public void AddPetl_ShouldReturnPetlBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPetl();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(PetlBuilder));
    }

    [TestMethod]
    public void AddPetl_ShouldContainServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPetl();

        // Assert
        Assert.AreSame(services, result.Services);
    }

    #endregion

    #region WithPipeline Tests

    [TestMethod]
    public void WithPipeline_ShouldRegisterPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithPipeline<DITestSource, DITestTarget>(pipeline =>
            {
                pipeline.WithStep("Test");
            });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetService<IPipeline<DITestSource, DITestTarget>>();

        // Assert
        Assert.IsNotNull(resolvedPipeline);
    }

    [TestMethod]
    public void WithPipeline_ShouldExecuteConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configureWasCalled = false;

        // Act
        services.AddPetl()
            .WithPipeline<DITestSource, DITestTarget>(pipeline =>
            {
                configureWasCalled = true;
                pipeline.WithStep("Test");
            });

        // Assert
        Assert.IsTrue(configureWasCalled);
    }

    [TestMethod]
    public void WithPipeline_ShouldReturnBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var petlBuilder = services.AddPetl();

        // Act
        var result = petlBuilder.WithPipeline<DITestSource, DITestTarget>(pipeline =>
        {
            pipeline.WithStep("Test");
        });

        // Assert
        Assert.AreSame(petlBuilder, result);
    }

    [TestMethod]
    public void WithPipeline_WithName_ShouldRegisterKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithPipeline<DITestSource, DITestTarget>("TestPipeline", pipeline =>
            {
                pipeline.WithStep("Test");
            });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetKeyedService<IPipeline<DITestSource, DITestTarget>>("TestPipeline");

        // Assert
        Assert.IsNotNull(resolvedPipeline);
    }

    [TestMethod]
    public void WithPipeline_MultiplePipelines_ShouldRegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithPipeline<DITestSource, DITestTarget>("Pipeline1", pipeline =>
            {
                pipeline.WithStep("Step1");
            })
            .WithPipeline<DITestSource, DifferentTarget>("Pipeline2", pipeline =>
            {
                pipeline.WithStep("Step2");
            });

        var provider = services.BuildServiceProvider();
        var pipeline1 = provider.GetKeyedService<IPipeline<DITestSource, DITestTarget>>("Pipeline1");
        var pipeline2 = provider.GetKeyedService<IPipeline<DITestSource, DifferentTarget>>("Pipeline2");

        // Assert
        Assert.IsNotNull(pipeline1);
        Assert.IsNotNull(pipeline2);
    }

    #endregion

    #region WithAutoMapping Tests

    [TestMethod]
    public void WithAutoMapping_ShouldRegisterAutoMappedPipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>();

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetService<IPipeline<DITestSource, DITestTarget>>();

        // Assert
        Assert.IsNotNull(resolvedPipeline);

        // Verify it works
        var source = new DITestSource { Name = "Test", Age = 25, Email = "test@example.com" };
        var target = new DITestTarget();
        resolvedPipeline.Exec(source, target);

        Assert.AreEqual("Test", target.Name);
        Assert.AreEqual(25, target.Age);
        Assert.AreEqual("test@example.com", target.Email);
    }

    [TestMethod]
    public void WithAutoMapping_WithName_ShouldRegisterKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>("AutoMapper");

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetKeyedService<IPipeline<DITestSource, DITestTarget>>("AutoMapper");

        // Assert
        Assert.IsNotNull(resolvedPipeline);
    }

    [TestMethod]
    public void WithAutoMapping_WithFilter_ShouldApplyFilter()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>((source, target, value) =>
            {
                // Only copy non-empty strings
                if (value is string str)
                    return !string.IsNullOrEmpty(str);
                return true;
            });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetService<IPipeline<DITestSource, DITestTarget>>();

        // Assert
        Assert.IsNotNull(resolvedPipeline);

        var source = new DITestSource { Name = "Test", Age = 25, Email = "" };
        var target = new DITestTarget { Email = "original@example.com" };
        resolvedPipeline.Exec(source, target);

        Assert.AreEqual("Test", target.Name);
        Assert.AreEqual(25, target.Age);
        Assert.AreEqual("original@example.com", target.Email); // Should remain unchanged
    }

    [TestMethod]
    public void WithAutoMapping_WithNameAndFilter_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>("FilteredMapper", (source, target, value) => value != null);

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetKeyedService<IPipeline<DITestSource, DITestTarget>>("FilteredMapper");

        // Assert
        Assert.IsNotNull(resolvedPipeline);
    }

    [TestMethod]
    public void WithAutoMapping_WithConfigure_ShouldAddAdditionalSteps()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>(pipeline =>
            {
                pipeline.WithStep("PostProcessing")
                    .Transform((source, target) => target.Name = target.Name.ToUpper());
            });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetService<IPipeline<DITestSource, DITestTarget>>();

        // Assert
        Assert.IsNotNull(resolvedPipeline);
        Assert.AreEqual(2, resolvedPipeline.StepCount); // AutoMap + PostProcessing

        var source = new DITestSource { Name = "test", Age = 25 };
        var target = new DITestTarget();
        resolvedPipeline.Exec(source, target);

        Assert.AreEqual("TEST", target.Name); // Transformed to uppercase
        Assert.AreEqual(25, target.Age);
    }

    [TestMethod]
    public void WithAutoMapping_WithNameAndConfigure_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>("CustomMapper", pipeline =>
            {
                pipeline.WithStep("Extra")
                    .Transform((source, target) => target.Email = target.Email.ToLower());
            });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetKeyedService<IPipeline<DITestSource, DITestTarget>>("CustomMapper");

        // Assert
        Assert.IsNotNull(resolvedPipeline);
        Assert.AreEqual(2, resolvedPipeline.StepCount);
    }

    [TestMethod]
    public void WithAutoMapping_WithFilterAndConfigure_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>(
                filter: (source, target, value) => value != null,
                configure: pipeline =>
                {
                    pipeline.WithStep("Extra")
                        .Transform((source, target) => target.Name = target.Name + "!");
                });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetService<IPipeline<DITestSource, DITestTarget>>();

        // Assert
        Assert.IsNotNull(resolvedPipeline);
        Assert.AreEqual(2, resolvedPipeline.StepCount);

        var source = new DITestSource { Name = "Test" };
        var target = new DITestTarget();
        resolvedPipeline.Exec(source, target);

        Assert.AreEqual("Test!", target.Name);
    }

    [TestMethod]
    public void WithAutoMapping_WithAllOptions_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPetl()
            .WithAutoMapping<DITestSource, DITestTarget>(
                name: "FullMapper",
                filter: (source, target, value) => value != null,
                configure: pipeline =>
                {
                    pipeline.WithStep("Transform")
                        .Transform((source, target) => target.Name = target.Name.ToUpper());
                });

        var provider = services.BuildServiceProvider();
        var resolvedPipeline = provider.GetKeyedService<IPipeline<DITestSource, DITestTarget>>("FullMapper");

        // Assert
        Assert.IsNotNull(resolvedPipeline);
        Assert.AreEqual(2, resolvedPipeline.StepCount);

        var source = new DITestSource { Name = "test", Age = 30 };
        var target = new DITestTarget();
        resolvedPipeline.Exec(source, target);

        Assert.AreEqual("TEST", target.Name);
        Assert.AreEqual(30, target.Age);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void Pipeline_ShouldBeResolvable_FromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPetl()
            .WithPipeline<DITestSource, DITestTarget>(pipeline =>
            {
                pipeline.WithAutoMapStep();
            });

        var provider = services.BuildServiceProvider();

        // Act
        var pipeline = provider.GetRequiredService<IPipeline<DITestSource, DITestTarget>>();

        // Assert
        Assert.IsNotNull(pipeline);

        var source = new DITestSource { Name = "Integration", Age = 42 };
        var target = new DITestTarget();
        pipeline.Exec(source, target);

        Assert.AreEqual("Integration", target.Name);
        Assert.AreEqual(42, target.Age);
    }

    [TestMethod]
    public void KeyedPipeline_ShouldBeResolvable_WithKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPetl()
            .WithPipeline<DITestSource, DITestTarget>("KeyedPipeline", pipeline =>
            {
                pipeline.WithAutoMapStep();
            });

        var provider = services.BuildServiceProvider();

        // Act
        var pipeline = provider.GetRequiredKeyedService<IPipeline<DITestSource, DITestTarget>>("KeyedPipeline");

        // Assert
        Assert.IsNotNull(pipeline);

        var source = new DITestSource { Name = "Keyed", Age = 99 };
        var target = new DITestTarget();
        pipeline.Exec(source, target);

        Assert.AreEqual("Keyed", target.Name);
        Assert.AreEqual(99, target.Age);
    }

    #endregion
}


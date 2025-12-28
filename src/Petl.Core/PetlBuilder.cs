using Microsoft.Extensions.DependencyInjection;

namespace Petl;

/// <summary>
/// Builder for configuring Petl pipelines in dependency injection
/// </summary>
public class PetlBuilder
{
    /// <summary>
    /// Gets the service collection
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the PetlBuilder class
    /// </summary>
    /// <param name="services">The service collection to register pipelines with</param>
    public PetlBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}


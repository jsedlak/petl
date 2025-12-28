using Microsoft.Extensions.DependencyInjection;

namespace Petl;

/// <summary>
/// Extension methods for registering Petl pipelines with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Petl pipeline support to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>A PetlBuilder for configuring pipelines</returns>
    public static PetlBuilder AddPetl(this IServiceCollection services)
    {
        return new PetlBuilder(services);
    }
}


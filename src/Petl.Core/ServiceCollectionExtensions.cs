using Microsoft.Extensions.DependencyInjection;
using Petl.Fluent;

namespace Petl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPetl(this IServiceCollection services, Action<PetlServicesBuilder> configure)
    {
        services.AddScoped<IRequestProcessor, RequestProcessor>();

        configure(new PetlServicesBuilder(services));

        return services;
    }
}

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Petl.Mediator.Fluent;

public class PetlServicesBuilder
{
    private IServiceCollection _services;

    internal PetlServicesBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public PetlServicesBuilder WithRequestProcessor<TRequestProcessor>() where TRequestProcessor : class, IRequestProcessor
    {
        _services.AddScoped<IRequestProcessor, TRequestProcessor>();

        return this;
    }

    public PetlServicesBuilder WithResponseDispatcher<TResponseDispatcher>() where TResponseDispatcher : class, IResponseDispatcher
    {
        _services.AddScoped<IResponseDispatcher, TResponseDispatcher>();

        return this;
    }

    public PetlServicesBuilder RegisterRequestHandlers(Assembly assembly)
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(m => m
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            );

        foreach(var handlerType in handlerTypes)
        {
            var baseTypes = handlerType.GetInterfaces().Where(m => m.IsGenericType
                && m.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach(var baseType in baseTypes)
            {
                _services.AddScoped(baseType, handlerType);
            }
        }

        return this;
    }

    public PetlServicesBuilder RegisterResponseHandlers(Assembly assembly)
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(m => m
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResponseHandler<>))
            );

        foreach (var handlerType in handlerTypes)
        {
            var baseTypes = handlerType.GetInterfaces().Where(m => m.IsGenericType
                && m.GetGenericTypeDefinition() == typeof(IResponseHandler<>));

            foreach (var baseType in baseTypes)
            {
                _services.AddScoped(baseType, handlerType);
            }
        }

        return this;
    }
}

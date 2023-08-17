using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TinyLink.Hits.Abstractions.Services;
using TinyLink.Hits.Services;

namespace TinyLink.Hits.ExtensionMethods;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddTinyLinkHits(this IServiceCollection services)
    {
        services.TryAddScoped<IHitsService, HitsService>();
        return services;
    }

}
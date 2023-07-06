using Microsoft.Extensions.DependencyInjection;
using TinyLink.ShortLinks.Abstractions.Repositories;
using TinyLink.ShortLinks.ExtensionMethods;

namespace TinyLink.ShortLinks.TableStorage.ExtensionMethods;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddTinyLinkShortLinksWithTableStorage(this IServiceCollection services)
    {
        services.AddTinyLinkShortLinks();
        services.AddScoped<IShortLinksRepository, ShortLinksRepository>();
        return services;
    }

}
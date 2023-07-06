using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TinyLink.ShortLinks.Abstractions.Services;
using TinyLink.ShortLinks.Services;

namespace TinyLink.ShortLinks.ExtensionMethods;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddTinyLinkShortLinks(this IServiceCollection services)
    {
        services.TryAddScoped<IShortLinksService, ShortLinksService>();
        return services;
    }

}
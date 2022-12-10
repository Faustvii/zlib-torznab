using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Zlib.Torznab.Models.Settings;
using Zlib.Torznab.Services.Elastic;
using Zlib.Torznab.Services.Ipfs;
using Zlib.Torznab.Services.Metadata;
using Zlib.Torznab.Services.Torrents;
using Zlib.Torznab.Services.Torznab;

namespace Zlib.Torznab.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceLayer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<ITorrentService, TorrentService>();
        services.AddHttpClient<IIpfsGateway, IpfsGateway>();
        services.AddScoped<ITorznabService, TorznabService>();
        services.AddScoped<IMetadataService, MetadataService>();
        services.AddScoped<IElasticService, ElasticService>();
        return services.AddElastic(configuration);
    }

    public static IServiceCollection AddElastic(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var elasticSettings = configuration.GetSection(ElasticSettings.Key).Get<ElasticSettings>();
        if (elasticSettings == null)
            throw new NotSupportedException("Elastic search configuration is required");

        services.AddSingleton<IConnectionSettingsValues, ConnectionSettings>(
            (_) => new ConnectionSettings(new Uri(elasticSettings.Server)).DefaultIndex("books")
        );
        services.AddSingleton<IElasticClient, ElasticClient>();
        return services;
    }
}

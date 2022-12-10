using MonoTorrent.Connections.TrackerServer;
using Zlib.Torznab.Models.Queues;
using Zlib.Torznab.Models.Settings;
using Zlib.Torznab.Presentation.API.Core;
using Zlib.Torznab.Presentation.API.HostedServices;

namespace Zlib.Torznab.Presentation.API;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationSettings(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApplicationSettings>(configuration.GetSection(ApplicationSettings.Key));
        services.Configure<IpfsSettings>(configuration.GetSection(IpfsSettings.Key));
        services.Configure<TorrentSettings>(configuration.GetSection(TorrentSettings.Key));
        services.Configure<TorznabSettings>(configuration.GetSection(TorznabSettings.Key));
        services.Configure<MetadataSettings>(configuration.GetSection(MetadataSettings.Key));
        return services;
    }

    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<HostedTorrentService>();
        services.AddHostedService<HostedTrackerService>();
        services.AddHostedService<HostedBackgroundJobPoolService>();
        services.AddHostedService<HostedDatabaseUpdater>();
        services.AddHostedService<HostedIndexUpdater>();

        return services;
    }

    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundJobPool>(new DefaultBackgroundJobPool(30));
        services.AddSingleton<ITrackerListener, APITrackerListener>();
        services.AddSingleton<APITrackerListener>();

        return services;
    }
}

using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using MonoTorrent.Client;
using Zlib.Torznab.Services.Torrents;
using DomainTorrentSettings = Zlib.Torznab.Models.Settings.TorrentSettings;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public class HostedTorrentService : IHostedService
{
    private readonly ITorrentService _torrentService;
    private readonly DomainTorrentSettings _torrentSettings;

    public HostedTorrentService(
        ITorrentService torrentService,
        IOptions<DomainTorrentSettings> options
    )
    {
        _torrentService = torrentService;
        _torrentSettings = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await BindMonoTorrentToSpecificInterface();
        await _torrentService.GetEngine().StartAllAsync();
    }

    private async Task BindMonoTorrentToSpecificInterface()
    {
        // await Task.Delay(10000);
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            var ipProperties = nic.GetIPProperties();
            Console.WriteLine(
                $"{nic.Id} - {nic.Name} - {nic.Description} - {nic.NetworkInterfaceType} - {ipProperties.UnicastAddresses[0].Address}"
            );
            if (
                string.Equals(
                    _torrentSettings.NetworkInterface,
                    nic.Id,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                var ipAddr = ipProperties.UnicastAddresses.FirstOrDefault(
                    x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                );
                if (ipAddr is null)
                {
                    Console.WriteLine(
                        $"We could not find an IPv4 address for {_torrentSettings.NetworkInterface}"
                    );
                    continue;
                }
                Console.WriteLine($"Binding MonoTorrent to {nic.Id} with ip {ipAddr.Address}");
                var ipEndpoint = new IPEndPoint(ipAddr.Address, _torrentSettings.Port);
                var engine = _torrentService.GetEngine();
                var settingsBuilder = new EngineSettingsBuilder(engine.Settings)
                {
                    ListenEndPoint = ipEndpoint,
                };
                await engine.UpdateSettingsAsync(settingsBuilder.ToSettings());
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _torrentService.GetEngine().StopAllAsync();
    }
}

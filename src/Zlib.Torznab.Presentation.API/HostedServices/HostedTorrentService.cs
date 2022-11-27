using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using MonoTorrent.Client;
using DomainTorrentSettings = Zlib.Torznab.Models.Settings.TorrentSettings;

namespace Zlib.Torznab.Presentation.API.HostedServices;

public class HostedTorrentService : IHostedService
{
    private readonly ClientEngine _client;
    private readonly DomainTorrentSettings _torrentSettings;

    public HostedTorrentService(ClientEngine client, IOptions<DomainTorrentSettings> options)
    {
        _client = client;
        _torrentSettings = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await BindMonoTorrentToSpecificInterface();
        await _client.StartAllAsync();
    }

    private async Task BindMonoTorrentToSpecificInterface()
    {
        await Task.Delay(10000);
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
                var ipEndpoint = new IPEndPoint(ipAddr.Address, 0);
                var settingsBuilder = new EngineSettingsBuilder(_client.Settings)
                {
                    ListenEndPoint = ipEndpoint,
                };
                await _client.UpdateSettingsAsync(settingsBuilder.ToSettings());
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _client.StopAllAsync();
    }
}

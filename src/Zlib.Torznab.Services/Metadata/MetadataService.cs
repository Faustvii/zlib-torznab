using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpCompress.Common;
using SharpCompress.Readers;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Settings;

namespace Zlib.Torznab.Services.Metadata;

public partial class MetadataService : IMetadataService
{
    [GeneratedRegex(
        @".*href=\""(?<fileName>fiction\.rar).*?right.+?(?<date>\d{4}-\d{2}-\d{2})",
        RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 1000
    )]
    private static partial Regex NewestFictionDumpDateParser();

    [GeneratedRegex(
        @".*href=\""(?<fileName>libgen\.rar).*?right.+?(?<date>\d{4}-\d{2}-\d{2})",
        RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 1000
    )]
    private static partial Regex NewestLibgenDumpDateParser();

    private readonly HttpClient _httpClient;
    private readonly IMetadataRepository _metadataRepository;
    private readonly ILogger<MetadataService> _logger;
    private readonly MetadataSettings _metadataSettings;

    public MetadataService(
        HttpClient httpClient,
        IMetadataRepository metadataRepository,
        IOptions<MetadataSettings> options,
        ILogger<MetadataService> logger
    )
    {
        _httpClient = httpClient;
        _metadataRepository = metadataRepository;
        _logger = logger;
        _metadataSettings = options.Value;
    }

    public async Task ImportLatestData(CancellationToken cancellationToken)
    {
        var metadata = await _metadataRepository.GetMetadata();
        var (fiction, libgen) = await GetLatestDumpMetadata(cancellationToken);
        if (fiction.DumpDate > DateOnly.FromDateTime(metadata.LastFictionImport))
        {
            _logger.LogInformation("Libgen fiction outdated");
            var workingDirectory = _metadataSettings.WorkingDirectory;
            var cleanSqlName = Path.Combine(workingDirectory, "fiction_clean.sql");
            var sqlFileName = await GetDumpData(fiction, cleanSqlName, cancellationToken);
            CleanFiction(sqlFileName, cleanSqlName);
            await ImportSqlFileToDatabase(cleanSqlName, cancellationToken);
            metadata.LatestUpdate = metadata.LastFictionImport = DateTime.UtcNow;
            await _metadataRepository.UpdateMetadata(metadata);
        }

        if (libgen.DumpDate > DateOnly.FromDateTime(metadata.LastLibgenImport))
        {
            var workingDirectory = _metadataSettings.WorkingDirectory;
            var cleanSqlName = Path.Combine(workingDirectory, "libgen_clean.sql");
            var sqlFileName = await GetDumpData(libgen, cleanSqlName, cancellationToken);
            CleanLibgen(sqlFileName, cleanSqlName);
            await ImportSqlFileToDatabase(cleanSqlName, cancellationToken);
            metadata.LatestUpdate = metadata.LastLibgenImport = DateTime.UtcNow;
            await _metadataRepository.UpdateMetadata(metadata);
        }
    }

    private async Task<string> GetDumpData(
        LibgenDump dump,
        string cleanSqlName,
        CancellationToken cancellationToken
    )
    {
        await DownloadDataDump(dump, cancellationToken);
        var sqlFileName = await ExtractArchive(dump);
        if (File.Exists(cleanSqlName))
            File.Delete(cleanSqlName);
        return sqlFileName;
    }

    private async Task<string> ExtractArchive(LibgenDump libgenDump)
    {
        _logger.LogInformation("Starting extraction of dbdump {dbdump}", libgenDump.FileName);

        await using var stream = File.OpenRead(
            Path.Combine(_metadataSettings.WorkingDirectory, libgenDump.FileName)
        );
        using var reader = ReaderFactory.Open(stream);
        var outputDirectory = Path.Combine(_metadataSettings.WorkingDirectory, "output/");
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        reader.WriteAllToDirectory(
            outputDirectory,
            options: new ExtractionOptions { PreserveFileTime = true, Overwrite = true }
        );

        _logger.LogInformation("Finished extraction of dbdump {dbdump}", libgenDump.FileName);
        return Path.Combine(outputDirectory, reader.Entry.Key);
    }

    private async Task DownloadDataDump(LibgenDump libgenDump, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting download of dbdump {dbdump}", libgenDump.FileName);
        await using var httpStream = await _httpClient.GetStreamAsync(
            $"{_metadataSettings.Libgen.DumpDirectoryUrl}{libgenDump.FileName}",
            cancellationToken
        );
        await using var file = File.Create(
            Path.Combine(_metadataSettings.WorkingDirectory, libgenDump.FileName)
        );
        CopyStream(httpStream, file);
        _logger.LogInformation("Finished download of dbdump {dbdump}", libgenDump.FileName);
    }

    private async Task<(LibgenDump Fiction, LibgenDump Libgen)> GetLatestDumpMetadata(
        CancellationToken cancellationToken
    )
    {
        var response = await _httpClient.GetAsync(
            $"{_metadataSettings.Libgen.DumpDirectoryUrl}",
            cancellationToken
        );
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var fictionMatch = NewestFictionDumpDateParser().Matches(responseString);
        var libgenMatch = NewestLibgenDumpDateParser().Matches(responseString);
        DateOnly.TryParse(
            fictionMatch[0].Groups["date"].Value,
            CultureInfo.InvariantCulture,
            out var newestFictionDumpDate
        );
        DateOnly.TryParse(
            libgenMatch[0].Groups["date"].Value,
            CultureInfo.InvariantCulture,
            out var newestLibgenDumpDate
        );

        return (
            new(fictionMatch[0].Groups["fileName"].Value, newestFictionDumpDate),
            new(libgenMatch[0].Groups["fileName"].Value, newestLibgenDumpDate)
        );
    }

    private record LibgenDump(string FileName, DateOnly DumpDate);

    private void CleanFiction(string inputFile, string outputFile)
    {
        StripUselessFromFile(
            inputFile,
            outputFile,
            (x) =>
                x.StartsWith(
                    "INSERT INTO `fiction_description`",
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }

    private void CleanLibgen(string inputFile, string outputFile)
    {
        var lineStartsToDiscard = new[]
        {
            "INSERT INTO `description`",
            "INSERT INTO `description_edited`",
            "INSERT INTO `topics`",
            "INSERT INTO `updated_edited`",
        };

        StripUselessFromFile(
            inputFile,
            outputFile,
            (line) =>
                lineStartsToDiscard.Any(x => line.StartsWith(x, StringComparison.OrdinalIgnoreCase))
        );
    }

    private async Task ImportSqlFileToDatabase(
        string inputFile,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Starting import of {file}", inputFile);

        var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = @"mysql",
                Arguments = string.Format(
                    CultureInfo.InvariantCulture,
                    "-C -B --host={0} -P {1} --user={2} --password={3} --database={4} -e \"\\. {5}\"",
                    _metadataSettings.Connection.Server,
                    3306,
                    _metadataSettings.Connection.User,
                    _metadataSettings.Connection.Password,
                    _metadataSettings.Connection.Database,
                    inputFile
                ),
                ErrorDialog = false,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Environment.CurrentDirectory,
            }
        );

        if (process is null)
            return;

        process.OutputDataReceived += (o, e) => Console.Out.WriteLine(e.Data);
        process.ErrorDataReceived += (o, e) => Console.Error.WriteLine(e.Data);
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.StandardInput.Close();
        await process.WaitForExitAsync(cancellationToken);
        _logger.LogInformation("Finished import of {file}", inputFile);
    }

    private void StripUselessFromFile(
        string inputFile,
        string outputFile,
        Func<string, bool> removeLinesMatching
    )
    {
        _logger.LogInformation("Starting to clean {file}", inputFile);

        using var reader = new StreamReader(inputFile);
        using var writer = new StreamWriter(outputFile);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (removeLinesMatching(line))
                continue;
            writer.WriteLine(line);
        }
        _logger.LogInformation("Finished cleaning of {file}", inputFile);
    }

    /// <summary>
    /// Copies the contents of input to output. Doesn't close either stream.
    /// </summary>
    private static void CopyStream(Stream input, Stream output)
    {
        var buffer = new byte[8 * 1024];
        int len;
        while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, len);
        }
    }
}
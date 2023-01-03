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
    private readonly SemaphoreSlim _semaphore = new(1);

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
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
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
                await PostProcess("fiction", cancellationToken);
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
                await PostProcess("libgen", cancellationToken);
                metadata.LatestUpdate = metadata.LastLibgenImport = DateTime.UtcNow;
                await _metadataRepository.UpdateMetadata(metadata);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task PostProcess(string type, CancellationToken cancellationToken)
    {
        var sql = type switch
        {
            "fiction"
                => "CREATE INDEX TimeLastModified_TimeAdded "
                    + "ON fiction(TimeLastModified, TimeAdded); "
                    + "SELECT * FROM fiction LIMIT 1; "
                    + "SELECT * FROM fiction_hashes LIMIT 1; "
                    + "RENAME TABLE IF EXISTS libgenrs_fiction TO libgenrs_fiction_old; "
                    + "RENAME TABLE fiction TO libgenrs_fiction; "
                    + "RENAME TABLE IF EXISTS libgenrs_fiction_hashes TO libgenrs_fiction_hashes_old; "
                    + "RENAME TABLE fiction_hashes TO libgenrs_fiction_hashes; "
                    + "DROP TABLE IF EXISTS libgenrs_fiction_old; "
                    + "DROP TABLE IF EXISTS libgenrs_fiction_hashes_old; ",
            "libgen"
                => "CREATE INDEX TimeLastModified_TimeAdded "
                    + "ON updated(TimeLastModified, TimeAdded); "
                    + "SELECT * FROM updated LIMIT 1; "
                    + "SELECT * FROM hashes LIMIT 1; "
                    + "RENAME TABLE IF EXISTS libgenrs_updated TO libgenrs_updated_old; "
                    + "RENAME TABLE updated TO libgenrs_updated; "
                    + "RENAME TABLE IF EXISTS libgenrs_hashes TO libgenrs_hashes_old; "
                    + "RENAME TABLE hashes TO libgenrs_hashes; "
                    + "DROP TABLE IF EXISTS libgenrs_updated_old; "
                    + "DROP TABLE IF EXISTS libgenrs_hashes_old; ",
            _ => ""
        };
        var filePath = Path.Combine(_metadataSettings.WorkingDirectory, "temp.sql");
        await File.WriteAllTextAsync(filePath, sql, cancellationToken);
        await ImportSqlFileToDatabase(filePath, cancellationToken);
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
        var dumpFile = Path.Combine(_metadataSettings.WorkingDirectory, libgenDump.FileName);
        if (File.Exists(dumpFile))
        {
            var lastDump = File.GetLastWriteTimeUtc(dumpFile);
            if (DateOnly.FromDateTime(lastDump) == libgenDump.DumpDate)
            {
                _logger.LogInformation(
                    "We already have a dbdump of {dbdump} from this date downloaded",
                    libgenDump.FileName
                );
                return;
            }
        }
        _logger.LogInformation("Starting download of dbdump {dbdump}", libgenDump.FileName);

        await using var httpStream = await _httpClient.GetStreamAsync(
            $"{_metadataSettings.Libgen.DumpDirectoryUrl}{libgenDump.FileName}",
            cancellationToken
        );
        await using var file = File.Create(dumpFile);
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
        var lineStartsToDiscard = new[] { "INSERT INTO `fiction_description`", "  FULLTEXT KEY", };

        StripUselessFromFile(
            inputFile,
            outputFile,
            (line) =>
                lineStartsToDiscard.Any(
                    x => line.StartsWith(x, StringComparison.OrdinalIgnoreCase)
                ),
            (line) =>
                line.Replace(
                    "KEY `Language` (`Language`),",
                    "KEY `Language` (`Language`)",
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
            "  KEY `Generic` (`Generic`) USING BTREE",
            "  KEY `VisibleTimeAdded` (`Visible`,`TimeAdded`) USING BTREE,",
            "  KEY `TimeAdded` (`TimeAdded`) USING BTREE,",
            "  KEY `Topic` (`Topic`(3)) USING BTREE,",
            "  KEY `VisibleID` (`Visible`,`ID`) USING BTREE,",
            "  KEY `VisibleTimeLastModified` (`Visible`,`TimeLastModified`,`ID`) USING BTREE,",
            "  KEY `DOI_INDEX` (`Doi`) USING BTREE,",
            "  KEY `Identifier` (`Identifier`),",
            "  KEY `TimeLastModifiedID` (`TimeLastModified`,`ID`) USING BTREE,",
            "  FULLTEXT KEY",
        };

        StripUselessFromFile(
            inputFile,
            outputFile,
            (line) =>
                lineStartsToDiscard.Any(
                    x => line.StartsWith(x, StringComparison.OrdinalIgnoreCase)
                ),
            (line) =>
                line.Replace(
                    "  KEY `Extension` (`Extension`),",
                    "  KEY `Extension` (`Extension`)",
                    StringComparison.OrdinalIgnoreCase
                )
        );

        File.Move(outputFile, inputFile, overwrite: true);

        var stopLineOne = "/*!40000 ALTER TABLE `updated` ENABLE KEYS */;";
        var stopLineTwo = "UNLOCK TABLES;";
        var hashLine = "/*!40000 ALTER TABLE `hashes` ENABLE KEYS */;";
        using var reader = new StreamReader(inputFile);
        using var writer = new StreamWriter(outputFile);
        string? line;
        string? previousLine = null;
        var stop = false;
        var hasSeenHashes = false;
        while ((line = reader.ReadLine()) != null)
        {
            if (
                hasSeenHashes
                && string.Equals(line, stopLineTwo, StringComparison.Ordinal)
                && string.Equals(previousLine, stopLineOne, StringComparison.Ordinal)
            )
                stop = true;

            if (string.Equals(line, hashLine, StringComparison.Ordinal))
                hasSeenHashes = true;

            writer.WriteLine(line);
            previousLine = line;
            if (stop)
                break;
        }
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

        var hasErrored = false;

        process.OutputDataReceived += (o, e) => Console.Out.WriteLine(e.Data);
        process.ErrorDataReceived += (o, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;
            hasErrored = true;
            Console.Error.WriteLine(e.Data);
        };
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.StandardInput.Close();
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0 || hasErrored)
            throw new Exception($"Error during sql import {process.ExitCode}");

        _logger.LogInformation("Finished import of {file}", inputFile);
    }

    private void StripUselessFromFile(
        string inputFile,
        string outputFile,
        Func<string, bool> removeLinesMatching,
        Func<string, string>? replaceLine = null
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
            if (replaceLine != null)
                line = replaceLine(line);
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

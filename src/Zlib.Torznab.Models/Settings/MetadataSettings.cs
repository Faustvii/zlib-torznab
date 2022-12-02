namespace Zlib.Torznab.Models.Settings;

public class MetadataSettings
{
    public const string Key = "Settings:Metadata";

    public LibgenElement Libgen { get; set; } = new();
    public DbConnectionElement Connection { get; set; } = new();
    public string WorkingDirectory { get; set; } = string.Empty;

    public class DbConnectionElement
    {
        public string Server { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
    }

    public class LibgenElement
    {
        public string DumpDirectoryUrl { get; set; } = string.Empty;
    }
}

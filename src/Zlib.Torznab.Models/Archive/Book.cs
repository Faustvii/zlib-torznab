namespace Zlib.Torznab.Models.Archive;

public record Book(
    uint Id,
    string Md5,
    string Author,
    string Title,
    string Year,
    string Extension,
    DateTime TimeAdded,
    string IpfsCid,
    uint Filesize,
    string Pages,
    string Language,
    string Publisher
);

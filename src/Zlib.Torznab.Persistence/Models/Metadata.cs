using System.ComponentModel.DataAnnotations;

namespace Zlib.Torznab.Persistence.Models;

public class Metadata
{
    [Key]
    public int Id { get; set; }
    public DateTime LatestUpdate { get; set; }
    public DateTime LastLibgenImport { get; set; }
    public DateTime LastFictionImport { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Zlib.Torznab.Persistence.Models;

namespace Zlib.Torznab.Persistence;

public partial class ArchiveContext : DbContext
{
    public ArchiveContext() { }

    public ArchiveContext(DbContextOptions<ArchiveContext> options) : base(options) { }

    public DbSet<Fiction> Fictions => Set<Fiction>();
    public DbSet<FictionHash> FictionHashes => Set<FictionHash>();
    public DbSet<Libgen> Libgen => Set<Libgen>();
    public DbSet<LibgenHash> LibgenHashes => Set<LibgenHash>();
    public DbSet<Metadata> Metadata => Set<Metadata>();
    public DbSet<ZlibBook> ZlibBooks => Set<ZlibBook>();
    public DbSet<ZlibIpfs> ZlibIpfs => Set<ZlibIpfs>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        BuildMetaModel(modelBuilder);
        BuildFictionModel(modelBuilder);
        BuildLibgenModel(modelBuilder);
        BuildZlibModel(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }
#pragma warning disable MA0051

    private static void BuildZlibModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ZlibBook>(entity =>
        {
            entity.ToTable("books");
        });

        modelBuilder.Entity<ZlibIpfs>(entity =>
        {
            entity.ToTable("zlib_ipfs");
        });
    }

    private static void BuildMetaModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Metadata>(entity =>
        {
            entity.ToTable("metadata");
        });
    }

    private static void BuildFictionModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fiction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("fiction").UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Asin).HasColumnName("ASIN");

            entity.Property(e => e.GooglebookId).HasColumnName("GooglebookID");
            entity.Property(e => e.Md5).HasColumnName("MD5");
        });

        modelBuilder.Entity<FictionHash>(entity =>
        {
            entity.HasKey(e => e.Md5).HasName("PRIMARY");

            entity.ToTable("fiction_hashes").HasCharSet("ascii").UseCollation("ascii_general_ci");

            entity.Property(e => e.Md5).HasColumnName("md5");
            entity.Property(e => e.IpfsCid).HasColumnName("ipfs_cid");
        });
    }

    private static void BuildLibgenModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Libgen>(entity =>
        {
            entity.ToTable("updated").UseCollation("utf8mb3_general_ci");
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Asin).HasColumnName("ASIN");
            entity.Property(e => e.GooglebookId).HasColumnName("GooglebookID");
            entity.Property(e => e.Md5).HasColumnName("MD5");
        });

        modelBuilder.Entity<LibgenHash>(entity =>
        {
            entity.HasKey(e => e.Md5).HasName("PRIMARY");

            entity.ToTable("hashes").HasCharSet("utf8mb3").UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Md5).HasColumnName("md5");
            entity.Property(e => e.IpfsCid).HasColumnName("ipfs_cid");
        });
    }

#pragma warning restore MA0051
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

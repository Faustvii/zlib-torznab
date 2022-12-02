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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        BuildMetaModel(modelBuilder);
        BuildFictionModel(modelBuilder);
        BuildLibgenModel(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }
#pragma warning disable MA0051

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
            entity
                .Property(e => e.Asin)
                .HasMaxLength(10)
                .HasDefaultValueSql("''")
                .HasColumnName("ASIN");
            entity.Property(e => e.Author).HasMaxLength(300).HasDefaultValueSql("''");
            entity.Property(e => e.Commentary).HasMaxLength(500);
            entity.Property(e => e.Coverurl).HasMaxLength(200).HasDefaultValueSql("''");
            entity.Property(e => e.Edition).HasMaxLength(50).HasDefaultValueSql("''");
            entity.Property(e => e.Extension).HasMaxLength(10);

            entity
                .Property(e => e.GooglebookId)
                .HasMaxLength(45)
                .HasDefaultValueSql("''")
                .HasColumnName("GooglebookID");
            entity.Property(e => e.Identifier).HasMaxLength(400).HasDefaultValueSql("''");
            entity.Property(e => e.Language).HasMaxLength(45).HasDefaultValueSql("''");
            entity.Property(e => e.Library).HasMaxLength(50).HasDefaultValueSql("''");
            entity.Property(e => e.Locator).HasMaxLength(512).HasDefaultValueSql("''");
            entity
                .Property(e => e.Md5)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("MD5")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Pages).HasMaxLength(10).HasDefaultValueSql("''");
            entity.Property(e => e.Publisher).HasMaxLength(100).HasDefaultValueSql("''");
            entity.Property(e => e.Series).HasMaxLength(300).HasDefaultValueSql("''");
            entity
                .Property(e => e.TimeAdded)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity
                .Property(e => e.TimeLastModified)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("timestamp");
            entity.Property(e => e.Title).HasMaxLength(2000).HasDefaultValueSql("''");
            entity
                .Property(e => e.Visible)
                .HasMaxLength(3)
                .HasDefaultValueSql("''")
                .IsFixedLength();
            entity.Property(e => e.Year).HasMaxLength(10).HasDefaultValueSql("''");
        });

        modelBuilder.Entity<FictionHash>(entity =>
        {
            entity.HasKey(e => e.Md5).HasName("PRIMARY");

            entity.ToTable("fiction_hashes").HasCharSet("ascii").UseCollation("ascii_general_ci");

            entity.Property(e => e.Md5).HasMaxLength(32).IsFixedLength().HasColumnName("md5");
            entity
                .Property(e => e.IpfsCid)
                .HasMaxLength(62)
                .HasDefaultValueSql("''")
                .IsFixedLength()
                .HasColumnName("ipfs_cid");
        });
    }

    private static void BuildLibgenModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Libgen>(entity =>
        {
            entity.ToTable("updated").UseCollation("utf8mb3_general_ci");
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Title).HasMaxLength(2000).HasDefaultValueSql("''");
            entity.Property(e => e.Author).HasMaxLength(300).HasDefaultValueSql("''");
            entity.Property(e => e.Year).HasMaxLength(10).HasDefaultValueSql("''");
            entity.Property(e => e.Edition).HasMaxLength(50).HasDefaultValueSql("''");
            entity.Property(e => e.Publisher).HasMaxLength(100).HasDefaultValueSql("''");
            entity.Property(e => e.Pages).HasMaxLength(10).HasDefaultValueSql("''");
            entity.Property(e => e.Language).HasMaxLength(45).HasDefaultValueSql("''");
            entity.Property(e => e.Library).HasMaxLength(50).HasDefaultValueSql("''");

            entity.Property(e => e.Identifier).HasMaxLength(400).HasDefaultValueSql("''");
            entity
                .Property(e => e.Asin)
                .HasMaxLength(10)
                .HasDefaultValueSql("''")
                .HasColumnName("ASIN");
            entity
                .Property(e => e.GooglebookId)
                .HasMaxLength(45)
                .HasDefaultValueSql("''")
                .HasColumnName("GooglebookID");

            entity.Property(e => e.Commentary).HasMaxLength(500);
            entity.Property(e => e.Coverurl).HasMaxLength(200).HasDefaultValueSql("''");
            entity.Property(e => e.Extension).HasMaxLength(10);

            entity.Property(e => e.Locator).HasMaxLength(512).HasDefaultValueSql("''");
            entity
                .Property(e => e.Md5)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("MD5")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Series).HasMaxLength(300).HasDefaultValueSql("''");
            entity
                .Property(e => e.TimeAdded)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity
                .Property(e => e.TimeLastModified)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("timestamp");
            entity
                .Property(e => e.Visible)
                .HasMaxLength(3)
                .HasDefaultValueSql("''")
                .IsFixedLength();
        });

        modelBuilder.Entity<LibgenHash>(entity =>
        {
            entity.HasKey(e => e.Md5).HasName("PRIMARY");

            entity.ToTable("hashes").HasCharSet("utf8mb3").UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Md5).HasMaxLength(32).IsFixedLength().HasColumnName("md5");
            entity
                .Property(e => e.IpfsCid)
                .HasMaxLength(62)
                .HasDefaultValueSql("''")
                .IsFixedLength()
                .HasColumnName("ipfs_cid");
        });
    }

#pragma warning restore MA0051
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

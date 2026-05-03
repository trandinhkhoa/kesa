using System.Text.Json;
using Kesa.Models;
using Microsoft.EntityFrameworkCore;

namespace Kesa.Data;

/// <summary>
/// EF Core database context for candidate profiles and dynamic field definitions.
/// </summary>
public class KesaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new DbContext instance.
    /// </summary>
    /// <param name="options">Configured DbContext options.</param>
    public KesaDbContext(DbContextOptions<KesaDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// User records for audit relationships.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Candidate profile records with core columns and JSONB custom fields.
    /// </summary>
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();

    /// <summary>
    /// Dynamic field definitions used to validate custom field keys and values.
    /// </summary>
    public DbSet<DefaultFields> ProfileFieldDefinitions => Set<DefaultFields>();

    /// <summary>
    /// Configures entity mappings, indexes, constraints, and seed data.
    /// </summary>
    /// <param name="modelBuilder">Model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(1024);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<CandidateProfile>(entity =>
        {
            entity.ToTable("candidate_profiles");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.BirthDate)
                .IsRequired();

            entity.Property(e => e.Sex)
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(e => e.CustomFields)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedCandidateProfiles)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany(u => u.UpdatedCandidateProfiles)
                .HasForeignKey(e => e.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.BirthDate);
            entity.HasIndex(e => e.Sex);

            entity.HasIndex(e => e.CustomFields)
                .HasMethod("gin")
                .HasOperators("jsonb_path_ops");
        });

        modelBuilder.Entity<DefaultFields>(entity =>
        {
            entity.ToTable("profile_field_definitions");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(e => e.OptionsJson)
                .HasColumnType("jsonb");

            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Key).IsUnique();

            var addressFieldId = Guid.Parse("8ddc8f74-4d9d-4623-96f6-4adfa1a6ea09");
            var religionFieldId = Guid.Parse("620f56ef-edf6-4a18-84fd-72a19f887cbc");
            var marriageFieldId = Guid.Parse("4e32edf0-8be8-4d83-aec8-244fdb6aa8c5");
            var seedTimestamp = new DateTime(2026, 4, 25, 0, 0, 0, DateTimeKind.Utc);

            entity.HasData(
                new DefaultFields
                {
                    Id = addressFieldId,
                    Name = "Address",
                    Key = "address",
                    DataType = "String",
                    IsRequired = false,
                    IsActive = true,
                    OptionsJson = null,
                    CreatedBy = null,
                    CreatedAt = seedTimestamp,
                    UpdatedAt = seedTimestamp
                },
                new DefaultFields
                {
                    Id = religionFieldId,
                    Name = "Religion",
                    Key = "religion",
                    DataType = "Enum",
                    IsRequired = false,
                    IsActive = true,
                    OptionsJson = JsonSerializer.Serialize(new[] { "buddism", "christian", "others" }),
                    CreatedBy = null,
                    CreatedAt = seedTimestamp,
                    UpdatedAt = seedTimestamp
                },
                new DefaultFields
                {
                    Id = marriageFieldId,
                    Name = "Marriage",
                    Key = "marriage",
                    DataType = "Enum",
                    IsRequired = false,
                    IsActive = true,
                    OptionsJson = JsonSerializer.Serialize(new[] { "no", "married", "divoced", "widowed" }),
                    CreatedBy = null,
                    CreatedAt = seedTimestamp,
                    UpdatedAt = seedTimestamp
                });
        });
    }
}

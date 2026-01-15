using Microsoft.EntityFrameworkCore;
using LeadFlowAI.Domain.Entities;
using System.Text.Json;

namespace LeadFlowAI.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<LeadEvent> LeadEvents { get; set; } = null!;
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Domain);
            
            entity.Property(e => e.Config)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<TenantConfig>(v, (JsonSerializerOptions)null!) ?? new TenantConfig()
                );

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Domain).HasMaxLength(200).IsRequired();
        });

        // Lead
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("leads");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.DeduplicationHash);
            entity.HasIndex(e => e.ExternalId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.DeduplicationHash });

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PhoneNormalized).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.DeduplicationHash).HasMaxLength(100).IsRequired();

            entity.Property(e => e.ServiceMatch).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)
                );

            entity.Property(e => e.KeyDetails).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)
                );

            entity.Property(e => e.MissingQuestions).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)
                );

            entity.Property(e => e.RiskFlags).HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)
                );

            entity.Property(e => e.LLMResponseRaw).HasColumnType("text");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Leads)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeadEvent
        modelBuilder.Entity<LeadEvent>(entity =>
        {
            entity.ToTable("lead_events");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Actor).HasMaxLength(200);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IdempotencyRecord
        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable("idempotency_records");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.Key).HasMaxLength(500).IsRequired();
        });
    }
}

using System.Linq.Expressions;
using CadenceComponentLibraryAdmin.Domain.Common;
using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CompanyPart> CompanyParts => Set<CompanyPart>();
    public DbSet<AiDatasheetExtraction> AiDatasheetExtractions => Set<AiDatasheetExtraction>();
    public DbSet<AiExtractionEvidence> AiExtractionEvidenceItems => Set<AiExtractionEvidence>();
    public DbSet<CadenceBuildJob> CadenceBuildJobs => Set<CadenceBuildJob>();
    public DbSet<CadenceBuildArtifact> CadenceBuildArtifacts => Set<CadenceBuildArtifact>();
    public DbSet<LibraryVerificationReport> LibraryVerificationReports => Set<LibraryVerificationReport>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    public DbSet<ExternalImportSource> ExternalImportSources => Set<ExternalImportSource>();
    public DbSet<ExternalComponentImport> ExternalComponentImports => Set<ExternalComponentImport>();
    public DbSet<ExternalComponentAsset> ExternalComponentAssets => Set<ExternalComponentAsset>();
    public DbSet<ManufacturerPart> ManufacturerParts => Set<ManufacturerPart>();
    public DbSet<SymbolFamily> SymbolFamilies => Set<SymbolFamily>();
    public DbSet<PackageFamily> PackageFamilies => Set<PackageFamily>();
    public DbSet<FootprintVariant> FootprintVariants => Set<FootprintVariant>();
    public DbSet<OnlineCandidate> OnlineCandidates => Set<OnlineCandidate>();
    public DbSet<SupplierOffer> SupplierOffers => Set<SupplierOffer>();
    public DbSet<PartAlternate> PartAlternates => Set<PartAlternate>();
    public DbSet<PartDoc> PartDocs => Set<PartDoc>();
    public DbSet<PartChangeLog> PartChangeLogs => Set<PartChangeLog>();
    public DbSet<LibraryRelease> LibraryReleases => Set<LibraryRelease>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("dbo");

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteQueryFilters(builder);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var compare = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(compare, parameter);
            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}

using CadenceComponentLibraryAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadenceComponentLibraryAdmin.Infrastructure.Data.Configurations;

public sealed class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.ToTable("AdminAuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(80).IsRequired();
        builder.Property(x => x.TargetType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.TargetId).HasMaxLength(256).IsRequired();
        builder.Property(x => x.TargetName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OldValue).HasMaxLength(2000);
        builder.Property(x => x.NewValue).HasMaxLength(2000);
        builder.Property(x => x.Actor).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(128);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
    }
}

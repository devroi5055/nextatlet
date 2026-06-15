using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Organization;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Billing;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
        entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);

        //SIMPLE SCALARS
        entity.Property(e => e.IsServerManaged).IsRequired();
        entity.Property(e => e.AthleteSlotCount);

        //ENUM
        entity.Property(e => e.OrganizationTypeId).IsRequired().HasMaxLength(50);
        entity.Property(e => e.OrganizationTierId).IsRequired().HasMaxLength(50).HasDefaultValue(OrganizationTier.Free.Id);
        entity.Property(e => e.VisibilityStateId).IsRequired().HasMaxLength(50).HasDefaultValue(VisibilityState.Public.Id);
        entity.Property(e => e.VerificationStatusId).IsRequired().HasMaxLength(50).HasDefaultValue(VerificationStatus.Pending.Id);

        //OWNED TYPES
        entity.OwnsOne(e => e.Verification, v =>
        {
            v.Property(p => p.MethodId).HasMaxLength(50);
        });

        //RELATIONS 1:N
        entity.HasMany(e => e.Logins).WithOne(l => l.Organization).HasForeignKey(l => l.OrganizationId);

        //INDEXES
        entity.HasIndex(e => e.Slug).IsUnique();
        entity.HasIndex(e => e.OrganizationTypeId);
    }
}

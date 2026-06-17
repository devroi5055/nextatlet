using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Billing;
using NextAtlet.Domain.Enumerations.Organization;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<OrganizationProfile>
{
    public void Configure(EntityTypeBuilder<OrganizationProfile> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS

        //SIMPLE SCALARS
        entity.Property(e => e.IsServerManaged).IsRequired();
        entity.Property(e => e.AthleteSlotCount);

        //ENUM
        entity.Property(e => e.OrganizationTypeId).IsRequired().HasMaxLength(50);
        entity.Property(e => e.OrganizationTierId).IsRequired().HasMaxLength(50).HasDefaultValue(OrganizationTier.Free.Id);
        entity.Property(e => e.VerificationStatusId).IsRequired().HasMaxLength(50).HasDefaultValue(VerificationStatus.Pending.Id);

        //OWNED TYPES
        entity.OwnsOne(e => e.Verification, v =>
        {
            v.Property(p => p.MethodId).HasMaxLength(50);
            v.Property(p => p.CVR).HasMaxLength(8);
        });

        //RELATIONS 1:N

        //INDEXES
        entity.HasIndex(e => e.OrganizationTypeId);
    }
}

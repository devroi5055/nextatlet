using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class GuardianConsentConfiguration : IEntityTypeConfiguration<GuardianConsent>
{
    public void Configure(EntityTypeBuilder<GuardianConsent> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.TermsVersion).IsRequired().HasMaxLength(50);

        //SIMPLE SCALARS
        entity.Property(e => e.CreatedUtc).IsRequired();

        //ENUMS
        entity.Property(e => e.MethodId).IsRequired().HasMaxLength(30);

        //RELATIONS N:1
        entity.HasOne(e => e.AthleteSite).WithMany().HasForeignKey(e => e.AthleteProfileId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.Guardian).WithMany().HasForeignKey(e => e.GuardianUserId).OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => e.AthleteProfileId);
    }
}

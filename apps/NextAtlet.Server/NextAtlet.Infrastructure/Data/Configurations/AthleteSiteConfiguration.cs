using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class AthleteSiteConfiguration : IEntityTypeConfiguration<AthleteSite>
{
    public void Configure(EntityTypeBuilder<AthleteSite> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Slug).IsRequired().HasMaxLength(256);
        entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);

        //ENUMS
        entity.Property(e => e.SportId).IsRequired().HasMaxLength(50).HasDefaultValue(Sport.Judo.Id);
        entity.Property(e => e.DefaultLocaleId).IsRequired().HasMaxLength(2).HasDefaultValue(Locale.Da.Id);
        entity.Property(e => e.VisibilityStateId).IsRequired().HasMaxLength(20).HasDefaultValue(VisibilityState.Public.Id);
        entity.Property(e => e.ControlModeId).IsRequired().HasMaxLength(30).HasDefaultValue(ControlMode.AthleteControlled.Id);
        entity.Property(e => e.ConsentStateId).IsRequired().HasMaxLength(30).HasDefaultValue(ConsentState.NotRequired.Id);
        entity.Property(e => e.SelfTierId).HasMaxLength(50);

        //SIMPLE SCALARS
        entity.Property(e => e.DateOfBirth).IsRequired();



        //INDEXES
        entity.HasIndex(e => e.Slug).IsUnique();
        entity.HasIndex(e => e.SportId);
        entity.HasIndex(e => e.CreatedUtc).IsDescending();
    }
}

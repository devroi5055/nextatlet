using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.AthleteProfile;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class AthleteProfileConfiguration : IEntityTypeConfiguration<AthleteProfile>
{
    public void Configure(EntityTypeBuilder<AthleteProfile> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS

        //ENUMS
        entity.Property(e => e.SportId).IsRequired().HasMaxLength(50).HasDefaultValue(Sport.Judo.Id);
        entity.Property(e => e.ControlModeId).IsRequired().HasMaxLength(30).HasDefaultValue(ControlModes.AthleteControlled.Id);
        entity.Property(e => e.ConsentStateId).IsRequired().HasMaxLength(30).HasDefaultValue(ConsentStates.NotRequired.Id);
        entity.Property(e => e.SelfTierId).HasMaxLength(50);

        //SIMPLE SCALARS
        entity.Property(e => e.DateOfBirth).IsRequired();

        //INDEXES
        entity.HasIndex(e => e.SportId);
        entity.HasIndex(e => e.CreatedUtc).IsDescending();
    }
}

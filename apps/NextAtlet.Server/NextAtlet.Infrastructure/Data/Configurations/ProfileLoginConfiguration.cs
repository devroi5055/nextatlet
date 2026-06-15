using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Athlete;
using NextAtlet.Domain.Enumerations.AthleteProfile;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class ProfileLoginConfiguration : IEntityTypeConfiguration<ProfileLogin>
{
    public void Configure(EntityTypeBuilder<ProfileLogin> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS

        //ENUM
        entity.Property(e => e.StatusId).IsRequired().HasMaxLength(20);
        entity.Property(e => e.RoleId).IsRequired().HasMaxLength(50);

        //JSONB
        entity.Property(e => e.Permissions).HasJsonbConversion();

        //RELATIONS N:1
        entity.HasOne(e => e.User)
            .WithMany(u => u.ProfileLogins)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.AthleteSite)
            .WithMany(ap => ap.ProfileLogins)
            .HasForeignKey(e => e.AthleteProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        //INDEXES
        entity.HasIndex(e => new { e.UserId, e.AthleteProfileId }).IsUnique();
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.AthleteProfileId);
    }
}

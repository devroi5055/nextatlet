using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.AthleteProfile;
using NextAtlet.Domain.Entities.Organization;
using NextAtlet.Domain.Enumerations.Membership;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS

        //SIMPLE SCALARS
        entity.Property(e => e.EndDate);
        entity.Property(e => e.OccupiesSlot).IsRequired().HasDefaultValue(true);

        //ENUMS
        entity.Property(e => e.RoleId).IsRequired().HasMaxLength(50);
        entity.Property(e => e.statusId).IsRequired().HasMaxLength(20).HasDefaultValue(MembershipStatus.Active.Id);

        //RELATIONS N:1
        entity.HasOne<AthleteSite>()
            .WithMany()
            .HasForeignKey(e => e.AthleteProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        //INDEXES
        entity.HasIndex(e => e.AthleteProfileId);
        entity.HasIndex(e => e.OrganizationId);
    }
}

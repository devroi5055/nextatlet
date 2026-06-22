using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Individual;

namespace NextAtlet.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.RoleId).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(256);

        //SIMPLE SCALARS
        entity.Property(e => e.ExpiresUtc).IsRequired();
        entity.Property(e => e.AcceptedUtc);

        //ENUMS
        entity.Property(e => e.StatusId).IsRequired().HasMaxLength(20);

        //RELATIONS N:1
        entity.HasOne(e => e.TargetSite)
            .WithMany()
            .HasForeignKey(e => e.TargetSiteId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.InvitedBy)
            .WithMany()
            .HasForeignKey(e => e.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => new { e.Email, e.StatusId });
        entity.HasIndex(e => e.TargetSiteId);
    }
}

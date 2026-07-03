using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Entities.Approval;

namespace NextAtlet.Infrastructure.Persistence.Configurations;

public class ChangeRequestConfiguration : IEntityTypeConfiguration<ChangeRequest>
{
    public void Configure(EntityTypeBuilder<ChangeRequest> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.PreviewImageUrl).HasMaxLength(512);

        //SIMPLE SCALARS
        entity.Property(e => e.ThemeVersion).IsRequired().HasDefaultValue(1);
        entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        //JSONB
        entity.Property(e => e.ProposedLayout).HasJsonbConversion().IsRequired();

        //RELATIONS N:1
        entity.HasOne<IndividualProfile>()
            .WithMany()
            .HasForeignKey(e => e.TargetProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<OrganizationProfile>()
            .WithMany()
            .HasForeignKey(e => e.ProposingOrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ProposedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(e => e.Theme)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        //INDEXES
        entity.HasIndex(e => e.TargetProfileId);
        entity.HasIndex(e => e.ProposingOrganizationId);
    }
}

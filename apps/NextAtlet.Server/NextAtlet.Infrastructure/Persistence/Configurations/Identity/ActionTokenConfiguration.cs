using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Infrastructure.Persistence.Configurations;

public class ActionTokenConfiguration : IEntityTypeConfiguration<ActionToken>
{
    public void Configure(EntityTypeBuilder<ActionToken> entity)
    {
        //Keys — the Id is the link token.
        entity.HasKey(e => e.Id);

        //ENUMS — stored as the enum name (Invite/Consent/OrgEmailVerification).
        entity.Property(e => e.TypeId).IsRequired().HasMaxLength(40);

        //SIMPLE SCALARS
        entity.Property(e => e.ExpiresUtc).IsRequired();
        entity.Property(e => e.AcceptedUtc);

        //JSONB — typed polymorphic payload, round-trips by "type" discriminator (same as SectionData).
        entity.Property(e => e.Payload).HasJsonbConversion().IsRequired();

        //Relation 
        entity.HasOne<Site>().WithMany().HasForeignKey(e => e.TargetSiteId).OnDelete(DeleteBehavior.Cascade);

        //INDEXES — pending lookups by type (HasPendingInvite / CountPendingInvitesByEmail) + by site.
        entity.HasIndex(e => new { e.TypeId, e.AcceptedUtc });
        entity.HasIndex(e => e.TargetSiteId);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Verification;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class ClubOfficialConfiguration : IEntityTypeConfiguration<ClubOfficial>
{
    public void Configure(EntityTypeBuilder<ClubOfficial> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
        entity.Property(e => e.Phone).HasMaxLength(256);
        entity.Property(e => e.Email).HasMaxLength(256);

        //ENUMS
        entity.Property(e => e.RoleId).IsRequired().HasMaxLength(256).HasDefaultValue(ClubOfficialRole.Other.Id);

        //INDEXES
        entity.HasIndex(e => new { e.Source, e.SourceKey }).IsUnique();

    }
}

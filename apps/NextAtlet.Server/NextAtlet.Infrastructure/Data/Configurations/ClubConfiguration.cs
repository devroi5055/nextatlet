using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Verification;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
        entity.Property(e => e.Address).IsRequired().HasMaxLength(256);
        entity.Property(e => e.Source).IsRequired().HasMaxLength(256);
        entity.Property(e => e.SourceKey).IsRequired().HasMaxLength(256);

        //ENUMS
        entity.Property(e => e.CountryId).HasMaxLength(256);
        entity.Property(e => e.SportIds).HasMaxLength(10);

        //RELATIONS
        entity.HasMany(e => e.Officials).WithOne().HasForeignKey(o => o.ClubId);

        //INDEXES
        entity.HasIndex(e => new { e.Source, e.SourceKey }).IsUnique();

    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.ValueObjects.Theme.Builders;

namespace NextAtlet.Infrastructure.Data.Configurations;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    private static readonly Guid ClassicThemeId = new("11111111-1111-1111-1111-111111111111");

    public void Configure(EntityTypeBuilder<Theme> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
        entity.Property(e => e.PreviewImageUrl).HasMaxLength(512);

        //SIMPLE SCALARS
        entity.Property(e => e.RetiredUtc);
        entity.Ignore(e => e.IsRetired); // computed from RetiredUtc

        //JSONB
        entity.Property(e => e.Manifest).HasJsonbConversion().IsRequired();

        //SEED DATA
        entity.HasData(new Theme
        {
            Id = ClassicThemeId,
            Name = "Classic",
            Manifest = ClassicTheme.Manifest(),
        });
    }
}

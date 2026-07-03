using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextAtlet.Domain.Entities.Shared;
using NextAtlet.Domain.Entities.Identity;

namespace NextAtlet.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        //Keys
        entity.HasKey(e => e.Id);

        //STRINGS
        entity.Property(e => e.Email).IsRequired().HasMaxLength(256);

        //ENUMS
        entity.Property(e => e.AuthProviderId).HasMaxLength(256);

        //INDEXES
        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.AuthProviderId).IsUnique();
    }
}

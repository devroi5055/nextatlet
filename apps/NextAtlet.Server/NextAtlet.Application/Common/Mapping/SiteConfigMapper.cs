using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Common.Mapping;

/// <summary>
/// Maps the SiteConfig entity to its DTO. Shared by the read and update handlers so the
/// projection lives in one place.
/// </summary>
public static class SiteConfigMapper
{
    public static SiteConfigDto ToDto(SiteConfig config) => new()
    {
        Id = config.Id,
        AthleteProfileId = config.AthleteProfileId,
        IsDraft = config.IsDraft,
        Layout = config.Layout,
        GlobalSettings = config.GlobalSettings,
        Version = config.Version
    };
}

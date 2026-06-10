using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Common.Mapping;

/// <summary>
/// Maps the AthleteSiteSnapshot entity to its DTO. Shared by the read and update handlers so the
/// projection lives in one place.
/// </summary>
public static class AthleteSiteSnapshotMapper
{
    public static AthleteSiteSnapshotDto ToDto(AthleteSiteSnapshot snapshot) => new()
    {
        Id = snapshot.Id,
        AthleteProfileId = snapshot.AthleteProfileId,
        Layout = snapshot.Layout,
        GlobalSettings = snapshot.GlobalSettings,
        Version = snapshot.Version
    };
}

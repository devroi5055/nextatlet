using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Contracts.Sites.Response;
using NextAtlet.Domain.Entities.Sites;

namespace NextAtlet.Application.Common.Mapping;

/// <summary>
/// Maps the SiteSnapshot entity to its DTO. Shared by the read and update handlers so the
/// projection lives in one place.
/// </summary>
public static class SiteSnapshotMapper
{
    public static SiteSnapshotResponse ToDto(SiteSnapshot snapshot) => new()
    {
        Id = snapshot.Id,
        SiteId = snapshot.SiteId,
        Layout = snapshot.Layout,
        GlobalSettings = snapshot.GlobalSettings,
    };
}

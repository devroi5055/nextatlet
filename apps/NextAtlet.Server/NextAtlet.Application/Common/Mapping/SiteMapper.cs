using NextAtlet.Application.Common.Extensions;
using NextAtlet.Application.Contracts.Sites.Response;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Application.Common.Mapping;

/// <summary>Projects a <see cref="Site"/> entity onto the outbound <see cref="SiteResponse"/> contract.</summary>
public static class SiteMapper
{
    public static SiteResponse ToResponse(Site site) => new()
    {
        Id = site.Id,
        Slug = site.Slug,
        DisplayName = site.DisplayName,
        DefaultLocale = Locale.FromId(site.DefaultLocaleId).ToDto(),
        VisibilityState = VisibilityStates.FromId(site.VisibilityStateId).ToDto()
    };
}

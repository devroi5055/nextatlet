using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.DTOs;
using NextAtlet.Infrastructure.Data;

namespace NextAtlet.Application.Features.Athletes.Queries;

public class GetDraftConfigQuery
{
    private readonly NextAtletDbContext _context;

    public GetDraftConfigQuery(NextAtletDbContext context)
    {
        _context = context;
    }

    public async Task<SiteConfigDto> ExecuteAsync(Guid athleteProfileId)
    {
        var siteConfig = await _context.SiteConfigs
            .FirstOrDefaultAsync(sc => sc.AthleteProfileId == athleteProfileId && sc.State == "Draft");

        if (siteConfig == null)
            throw new InvalidOperationException($"Draft config not found for profile {athleteProfileId}");

        return new SiteConfigDto
        {
            Id = siteConfig.Id,
            AthleteProfileId = siteConfig.AthleteProfileId,
            State = siteConfig.State,
            Layout = siteConfig.Layout,
            GlobalSettings = siteConfig.GlobalSettings,
            Version = siteConfig.Version
        };
    }
}

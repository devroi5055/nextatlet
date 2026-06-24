using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Errors;
using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Common.Time;
using NextAtlet.Application.Features.ActionTokens.Models;
using NextAtlet.Application.Features.ActionTokens.Strategies;
using NextAtlet.Application.Features.Identity;
using NextAtlet.Domain.Common;
using NextAtlet.Domain.Entities.Consent;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Identity;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Organization;

public class InvitationStrategy : IActionTokenStrategy
{
    public ActionTokenType ActionTokenType => ActionTokenType.Invitation;
    public bool authRequired => true;

    private readonly ISiteRepository _siteRepository;
    private readonly ISiteLoginRepository _siteLoginRepository;

    public InvitationStrategy(
        ISiteRepository siteRepository,
        ISiteLoginRepository siteLoginRepository)
    {
        _siteRepository = siteRepository;
        _siteLoginRepository = siteLoginRepository;
    }

    public async Task<Result> ExecuteAsync(ActionToken token, User? actorUser, CancellationToken ct)
    {
        if (actorUser is null)
            throw new InvalidOperationException("User should always be authenticated to enter this strategy");
        
        var site = await _siteRepository.GetByIdAsync(token.TargetSiteId, ct);
        if (site is null)
            throw new InvalidOperationException("Should not be able to target non-exsisting site");

        var payload = (InvitePayload)token.Payload;

        RoleValidation(payload.RoleId, site.SiteTypeId);

        var siteLogin = SiteLogin.CreateActiveSiteLogin(actorUser.Id, site.Id, payload.RoleId);
        _siteLoginRepository.Add(siteLogin);


        return Result.Success();
    }

    private void RoleValidation(string roleId, string siteTypeId)
    {
        if (!SiteType.All.Contains(SiteType.FromId(siteTypeId)))
            throw new InvalidOperationException($"Invalid site type {siteTypeId} ");


        if (siteTypeId == SiteType.Individual.Id && !(IndividualRole.All.Contains(IndividualRole.FromId(roleId))) ||
            siteTypeId == SiteType.Organization.Id && !(OrganizationRole.All.Contains(OrganizationRole.FromId(roleId))))
            throw new InvalidOperationException($"Invalid role {roleId} for site type {siteTypeId}");

        return;
    }
}
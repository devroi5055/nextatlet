using MediatR;
using Microsoft.EntityFrameworkCore;
using NextAtlet.Application.Features.Individuals.Registration;
using NextAtlet.Application.Features.Invitations.Commands;
using NextAtlet.Application.Features.Organizations.Registration;
using NextAtlet.Application.Features.Organizations.Verification;
using NextAtlet.Domain.Entities.ClubRegistry;
using NextAtlet.Domain.Enumerations.Billing;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Organization;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Enumerations.Verification;
using NextAtlet.Infrastructure.Persistence;

namespace NextAtlet.Api.Seeding;

/// <summary>
/// Seeds a small but <i>structurally coherent</i> development dataset by replaying the real commands, so
/// every seeded site reaches a genuine domain state with all the entities that state implies — not just
/// bare rows. In particular, the "pending" states carry their live <c>ActionToken</c>s, exactly as a real
/// signup would leave them:
/// <list type="bullet">
///   <item>Adult individuals → consent <c>NotRequired</c> (publishable; no token).</item>
///   <item>Minor individuals → <c>PendingGuardianConsent</c> + a live <b>Consent</b> action token.</item>
///   <item>One minor also has a pending guardian <b>Invitation</b> action token.</item>
///   <item>Organizations → verification <c>Pending</c>; one has a live <b>OrgEmailVerification</b> token
///         (which requires a registry club + official, also seeded).</item>
/// </list>
/// Going through the commands means seeded data can never drift from real signup behaviour. Idempotent:
/// a no-op once any site exists.
/// </summary>
public static class DevelopmentDataSeeder
{
    private const string Domain = "seed.nextatlet.dk";

    private static string OwnerAuth(string slug) => $"seed|{slug}";
    private static string OwnerEmail(string slug) => $"{slug}@{Domain}";
    private static string GuardianEmail(string slug) => $"guardian-of-{slug}@{Domain}";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db     = scope.ServiceProvider.GetRequiredService<NextAtletDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevelopmentDataSeeder));

        if (await db.Sites.AnyAsync(cancellationToken))
            return; // already seeded — keep this idempotent

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await SeedIndividualsAsync(sender, logger, cancellationToken);
        await SeedOrganizationsAsync(sender, db, logger, cancellationToken);

        logger.LogInformation("Development data seeding complete.");
    }

    // ── Individuals ─────────────────────────────────────────────────────────────

    private static async Task SeedIndividualsAsync(ISender sender, ILogger logger, CancellationToken ct)
    {
        // Adults: consent not required → fully set-up, publishable profiles with no outstanding tokens.
        (string Name, string Slug, DateTime Dob)[] adults =
        [
            ("Ada Jensen",     "ada-jensen",     new DateTime(1996,  3, 12)),
            ("Bjørn Madsen",   "bjorn-madsen",   new DateTime(1990,  7,  4)),
            ("David Sørensen", "david-sorensen", new DateTime(1988,  1, 30)),
        ];
        foreach (var a in adults)
            await RegisterIndividualAsync(sender, a.Name, a.Slug, a.Dob, guardianEmail: null, logger, ct);

        // Minors (below the self-consent age): each lands in PendingGuardianConsent and the command issues
        // a live Consent action token addressed to the guardian's email.
        (string Name, string Slug, DateTime Dob)[] minors =
        [
            ("Clara Holm", "clara-holm", new DateTime(2011, 11, 22)), // ~15
            ("Emma Lund",  "emma-lund",  new DateTime(2012,  9,  9)), // ~14
        ];

        Guid? firstMinorSiteId = null;
        foreach (var m in minors)
        {
            var siteId = await RegisterIndividualAsync(sender, m.Name, m.Slug, m.Dob, GuardianEmail(m.Slug), logger, ct);
            firstMinorSiteId ??= siteId;

            // On the first minor, also issue a guardian *invitation* (a separate, owner-initiated flow
            // from consent) so the dataset has a live Invitation action token too.
            if (siteId is Guid sid && firstMinorSiteId == sid)
            {
                var invite = await sender.Send(new InviteToProfileCommand(
                    SiteId:               sid,
                    CallerAuthProviderId: OwnerAuth(m.Slug),
                    CallerEmail:          OwnerEmail(m.Slug),
                    Email:                GuardianEmail(m.Slug),
                    RoleId:               IndividualRole.Guardian.Id), ct);

                if (invite.IsFailure)
                    logger.LogWarning("Seeding guardian invite for '{Slug}' failed: {Code}", m.Slug, invite.Error!.Code);
            }
        }
    }

    private static async Task<Guid?> RegisterIndividualAsync(
        ISender sender, string name, string slug, DateTime dob, string? guardianEmail, ILogger logger, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterIndividualSiteSelfCommand(
            AuthProviderId:  OwnerAuth(slug),
            Email:           OwnerEmail(slug),
            DisplayName:     name,
            Slug:            slug,
            DateOfBirth:     dob,
            DefaultLocaleId: Locale.Da.Id,
            GuardianEmail:   guardianEmail), ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Seeding individual site '{Slug}' failed: {Code}", slug, result.Error!.Code);
            return null;
        }
        return result.Value!.Id;
    }

    // ── Organizations + club registry ────────────────────────────────────────────

    private static async Task SeedOrganizationsAsync(ISender sender, NextAtletDbContext db, ILogger logger, CancellationToken ct)
    {
        (string Name, string Slug, string TypeId)[] organizations =
        [
            ("Københavns Judoklub",          "kobenhavns-judoklub",            OrganizationType.Club.Id),
            ("Aarhus Judo Center",           "aarhus-judo-center",             OrganizationType.Club.Id),
            ("Odense Judo Akademi",          "odense-judo-akademi",            OrganizationType.Academy.Id),
            ("Nordsjællands Træningscenter", "nordsjaellands-traeningscenter", OrganizationType.TrainingCenter.Id),
            ("Danmarks Judo Landshold",      "danmarks-judo-landshold",        OrganizationType.NationalTeam.Id),
        ];

        Guid? firstOrgSiteId = null;
        foreach (var o in organizations)
        {
            var result = await sender.Send(new RegisterOrganizationSiteCommand(
                AuthProviderId:     OwnerAuth(o.Slug),
                Email:              OwnerEmail(o.Slug),
                Slug:               o.Slug,
                DisplayName:        o.Name,
                PlanTierId:         OrganizationTier.Free.Id,
                DefaultLocaleId:    Locale.Da.Id,
                OrganizationTypeId: o.TypeId), ct);

            if (result.IsFailure)
                logger.LogWarning("Seeding organization site '{Slug}' failed: {Code}", o.Slug, result.Error!.Code);
            else
                firstOrgSiteId ??= result.Value!.Id;
        }

        // Email-to-official verification is authority-checked against the trusted club registry, so seed a
        // matching registry club + official, then start verification for the first org → a live
        // OrgEmailVerification action token (the org stays Pending until the link is accepted).
        if (firstOrgSiteId is not Guid orgSiteId)
            return;

        var official = await SeedRegistryClubWithOfficialAsync(db, ct);

        var verification = await sender.Send(new SendOfficialEmailVerificationCommand(
            AuthProviderId: OwnerAuth(organizations[0].Slug),
            Email:          OwnerEmail(organizations[0].Slug),
            OrgSiteId:      orgSiteId,
            ClubOfficialId: official.Id), ct);

        if (verification.IsFailure)
            logger.LogWarning("Seeding org email verification failed: {Code}", verification.Error!.Code);
    }

    /// <summary>Seeds a registry club + chairman official (the authority basis the verification flow reads).</summary>
    private static async Task<ClubOfficial> SeedRegistryClubWithOfficialAsync(NextAtletDbContext db, CancellationToken ct)
    {
        var club = new Club
        {
            SourceKey       = "seed|kobenhavns-judoklub",
            Source          = "seed",
            CountryId       = Country.Denmark.Id,
            Name            = "Københavns Judoklub",
            Address         = "Judogade 1, 2100 København Ø",
            LastImportedUtc = DateTime.UtcNow,
            SportIds        = [Sport.Judo.Id],
        };
        db.Clubs.Add(club);

        var official = new ClubOfficial
        {
            ClubId = club.Id,
            Name   = "Formand Hansen",
            Email  = "formand@kobenhavns-judoklub.dk",
            Phone  = null,
            RoleId = ClubOfficialRole.Chairman.Id,
        };
        db.ClubOfficials.Add(official);

        await db.SaveChangesAsync(ct);
        return official;
    }
}

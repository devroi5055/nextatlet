//using NextAtlet.Application.Interfaces;
//using NextAtlet.Domain.ValueObjects;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace NextAtlet.Application.Services
//{
//    public interface IPerkResolver
//    {
//        Task<HashSet<string>> ResolveAsync(Guid athleteProfileId, CancellationToken ct);
//    }

//    // Application/Services/PerkResolver.cs
//    public class PerkResolver : IPerkResolver
//    {
//        private readonly IIndividualProfileRepository _profiles;
//        private readonly ISubscriptionRepository _subscriptions;
//        private readonly IMembershipRepository _memberships;

//        public async Task<HashSet<string>> ResolveAsync(Guid athleteProfileId, CancellationToken ct)
//        {
//            // 1. get the athlete's own plan capabilities
//            var subscription = await _subscriptions.GetActiveByProfileIdAsync(athleteProfileId, ct);
//            var selfCapabilities = subscription is null
//                ? PlanCapabilities.Free
//                : PlanCapabilities.ByPlanKey[subscription.PlanKey];

//            // 2. get the active club's plan capabilities (if any)
//            var clubMembership = await _memberships.GetActiveClubMembershipAsync(athleteProfileId, ct);
//            if (clubMembership is null)
//                return selfCapabilities;

//            var clubSubscription = await _subscriptions
//                .GetActiveByOrganizationIdAsync(clubMembership.OrganizationId, ct);
//            var clubCapabilities = clubSubscription is null
//                ? new HashSet<string>()
//                : PlanCapabilities.ByPlanKey[clubSubscription.PlanKey];

//            // 3. per-feature max — union covers OR/max for string capability keys
//            return selfCapabilities.Union(clubCapabilities).ToHashSet();
//        }
//    }
//}

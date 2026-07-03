//using FluentAssertions;
//using NextAtlet.Domain.Entities.Athlete;
//using NextAtlet.Domain.Enumerations;
//using NextAtlet.Domain.Enumerations.Enums.IndividualProfile;
//using System.Globalization;
//using Xunit;

namespace NextAtlet.Domain.Tests.Entities.Affiliation;

//public class MembershipTests
//{
//    private static Membership AnActiveMembership() => new()
//    {
//        IndividualProfileId = Guid.NewGuid(),
//        OrganizationId = Guid.NewGuid(),
//        Role = "competitor",
//        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
//        EndDate = null,
//        Status = "Active",
//        OccupiesSlot = true
//    };

//    [Fact]
//    public void OngoingMembership_HasNoEndDate()
//    {
//        var membership = AnActiveMembership();

//        membership.EndDate.Should().BeNull();
//        membership.Status.Should().Be("Active");
//    }

//    [Fact(Skip = "Confirm whether End() is an entity method or handled directly in a command.")]
//    public void End_SetsEndDateAndMarksInactive_ButRetainsRow()
//    {
//        var membership = AnActiveMembership();
//        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

//        membership.End(endDate);

//        membership.EndDate.Should().Be(endDate);
//        membership.Status.Should().Be("Inactive");
//    }

//    [Fact(Skip = "Confirm whether End() frees the occupied slot as part of ending.")]
//    public void End_FreesTheOccupiedSlot()
//    {
//        var membership = AnActiveMembership();

//        membership.End(DateOnly.FromDateTime(DateTime.UtcNow));

//        membership.OccupiesSlot.Should().BeFalse();
//    }
//}

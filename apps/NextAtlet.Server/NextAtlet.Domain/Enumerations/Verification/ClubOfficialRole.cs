using NextAtlet.Domain.Common;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Domain.Enumerations.Verification
{
    public class ClubOfficialRole : Enumeration
    {
        public static readonly ClubOfficialRole Chairman = new()
        {
            Id = "chairman",
            Title = new LocalizedText { Da = "Formand", En = "Chairman" },
            Description = new LocalizedText { Da = "Formand for klubben", En = "Chairman of the club" }
        };
        public static readonly ClubOfficialRole PostalAddress = new()
        {
            Id = "postal_address",
            Title = new LocalizedText { Da = "Postadresse", En = "Postal Address" },
            Description = new LocalizedText { Da = "Postadresse for klubben", En = "Postal Address of the club" }
        };
        public static readonly ClubOfficialRole Cashier = new()
        {
            Id = "cashier",
            Title = new LocalizedText { Da = "Kasserer", En = "Cashier" },
            Description = new LocalizedText { Da = "Kasserer for klubben", En = "Cashier of the club" }
        };
        public static readonly ClubOfficialRole Other = new()
        {
            Id = "other",
            Title = new LocalizedText { Da = "Andet", En = "Other" },
            Description = new LocalizedText { Da = "Anden rolle for klubben", En = "Other role for the club" }
        };


        public static IReadOnlyCollection<ClubOfficialRole> All => [Chairman, PostalAddress, Cashier, Other];

        public static ClubOfficialRole FromId(string id) =>
            All.FirstOrDefault(b => b.Id == id)
            ?? throw new ArgumentException($"Unknown club official role: '{id}'");
    }
}

using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.ClubRegistry;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Application.Abstractions.Services;

public interface IClubCanonicalizer
{
    ScrapedClub Canonicalize(ScrapedClub raw);
}
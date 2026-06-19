using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Entities.Verification;
using NextAtlet.Domain.Enumerations.Shared;
using NextAtlet.Domain.Enumerations.Verification;

namespace NextAtlet.Application.Interfaces.Services;

public interface IClubCanonicalizer
{
    ScrapedClub Canonicalize(ScrapedClub raw);
}
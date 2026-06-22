using NextAtlet.Application.Common.DTOs;
using NextAtlet.Application.Common.Results;
using NextAtlet.Domain.Enumerations.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Application.Interfaces.Strategies
{
    public interface IClubSourceStrategy
    {
        bool Supports(string sport, string country);
        IEnumerable<(string Sport, string Country)> Targets { get; }
        string Source { get; }
        Task<ScrapedClub[]> FetchAsync(CancellationToken ct);
    }
}

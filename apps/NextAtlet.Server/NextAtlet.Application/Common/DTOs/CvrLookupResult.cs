using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Application.Common.DTOs
{
    /// <summary>
    /// The subset of CVR register data the platform needs: confirms existence,
    /// supplies the official name and the register-sourced contact email used
    /// for authority verification (we send the verification link there, never to
    /// an address the registrant typed).
    /// </summary>
    public class CvrLookupResult
    {
        public required string CvrNumber { get; init; }
        public required string Name { get; init; }
        public string? ContactEmail { get; init; }   // register-sourced; may be absent
        public bool IsActive { get; init; }
    }
}

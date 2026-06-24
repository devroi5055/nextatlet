using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Application.Features.ActionTokens.Models
{
    public sealed record ActionTokenActor(
        string Email,
        string AuthProviderId
    );
}

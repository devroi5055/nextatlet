using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Domain.Common
{
    public abstract class Enumeration
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public string? Description { get; init; }

        public override string ToString() => Id;
    }
}

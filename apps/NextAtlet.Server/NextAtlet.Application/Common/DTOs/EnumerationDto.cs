using NextAtlet.Domain.ValueObjects;

namespace NextAtlet.Application.Common.DTOs;

public class EnumerationDto
{
    public string Id { get; set; } = default!;
    public LocalizedText Title { get; set; } = default!;
    public LocalizedText? Description { get; set; }
}


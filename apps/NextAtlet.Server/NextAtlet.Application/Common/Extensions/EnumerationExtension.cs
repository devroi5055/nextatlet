using NextAtlet.Application.Common.DTOs;
using NextAtlet.Domain.Common;

namespace NextAtlet.Application.Common.Extensions;

public static class EnumerationExtensions
{
    public static EnumerationDto ToDto(this Enumeration enumeration) => new()
    {
        Id = enumeration.Id,
        Title = enumeration.Title,
        Description = enumeration.Description
    };
}
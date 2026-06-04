using NextAtlet.Domain.Entities.Athlete;

namespace NextAtlet.Application.Abstractions.Persistence;

public interface IProfileLoginRepository
{
    void Add(ProfileLogin login);
}

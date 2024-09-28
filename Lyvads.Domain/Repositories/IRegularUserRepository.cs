using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IRegularUserRepository
{
    Task AddAsync(RegularUser regularUser);
    IQueryable<RegularUser> GetRegularUsers();
}

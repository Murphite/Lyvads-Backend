using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IRegularUserRepository
{
    Task AddAsync(RegularUser regularUser);
    IQueryable<RegularUser> GetRegularUsers();
    Task<RegularUser?> GetRegularUserByApplicationUserIdAsync(string applicationUserId);
    Task<RegularUser> GetByIdWithApplicationUser(string id);
}

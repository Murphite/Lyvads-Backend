using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IAdminRepository
{
    Task AddAsync(Admin admin);
}

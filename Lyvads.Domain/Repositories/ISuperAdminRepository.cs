

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface ISuperAdminRepository
{
    Task AddAsync(SuperAdmin superAdmin);

}


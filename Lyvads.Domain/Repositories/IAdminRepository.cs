using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;

namespace Lyvads.Domain.Repositories;

public interface IAdminRepository
{
    Task AddAsync(Admin admin);
    Task<ApplicationUser?> GetAdminByIdAsync(string adminUserId);
    Task<List<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser> GetByIdAsync(int id);
    Task AddAsync(ApplicationUser adminUser);
    Task UpdateAsync(ApplicationUser adminUser);
    Task<List<ApplicationUser>> GetAllAdminsAsync();
    Task AddAsync(AdminPermission permission);
    Task AddRoleAsync(AdminRole role);
    Task<AdminRole> GetRoleByNameAsync(string roleName);
    Task<AdminPermission?> GetByUserIdAsync(string userId);
    Task UpdateAsync(AdminPermission adminPermission);
    Task DeleteRelatedEntitiesAsync(ApplicationUser user);
    Task<ApplicationUser?> GetUserWithRelatedEntitiesAsync(string userId);
    Task<List<AdminRole>> GetAllRolesWithPermissionsAsync();
}

using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;


    public AdminRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task AddAsync(Admin admin)
    {
        await _context.Admins.AddAsync(admin);
        await _context.SaveChangesAsync();
    }


    public async Task<List<ApplicationUser>> GetAllAsync()
    {
        return await _context.Set<ApplicationUser>().ToListAsync();
    }

    public async Task<ApplicationUser> GetByIdAsync(int id)
    {
        return await _context.Set<ApplicationUser>().FindAsync(id);
    }

    public async Task AddAsync(ApplicationUser adminUser)
    {
        await _context.Set<ApplicationUser>().AddAsync(adminUser);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApplicationUser adminUser)
    {
        _context.Set<ApplicationUser>().Update(adminUser);
        await _context.SaveChangesAsync();
    }


    public async Task<List<ApplicationUser>> GetAllAdminsAsync()
    {
        var superAdmins = await _userManager.GetUsersInRoleAsync(RolesConstant.SuperAdmin);

        var admins = await _userManager.GetUsersInRoleAsync(RolesConstant.Admin);

        // Combine the two lists (Admin + SuperAdmin)
        var allAdmins = superAdmins.Concat(admins).ToList();

        return allAdmins;
    }


    public async Task AddAsync(AdminPermission permission)
    {
        await _context.AdminPermissions.AddAsync(permission);
        await _context.SaveChangesAsync();
    }

    // Get role by name
    public async Task<AdminRole> GetRoleByNameAsync(string roleName)
    {
        var roles =  await _context.AdminRoles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        
        return roles;        
    }

    // Add a new role to the database
    public async Task AddRoleAsync(AdminRole role)
    {
        await _context.AdminRoles.AddAsync(role);
        await _context.SaveChangesAsync(); 
    }
}

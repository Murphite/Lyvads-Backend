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
        var user = await _context.Set<ApplicationUser>().FindAsync(id);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        return user;
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

        // Log the counts to debug
        Console.WriteLine($"SuperAdmins Count: {superAdmins.Count}");
        Console.WriteLine($"Admins Count: {admins.Count}");

        var allAdmins = superAdmins.Concat(admins).ToList();

        return allAdmins;
    }

    public async Task<ApplicationUser?> GetAdminByIdAsync(string adminUserId)
    {
        // Check for Admin
        var admin = await _context.Admins
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.Id == adminUserId);

        if (admin != null)
        {
            var roles = await _userManager.GetRolesAsync(admin.ApplicationUser!);
            if (roles.Any(role => role.Equals(RolesConstant.Admin, StringComparison.OrdinalIgnoreCase)))
            {
                return admin.ApplicationUser;
            }
        }

        // Check for SuperAdmin
        var superAdmin = await _context.SuperAdmins
            .Include(sa => sa.ApplicationUser)
            .FirstOrDefaultAsync(sa => sa.Id == adminUserId);

        if (superAdmin != null)
        {
            var roles = await _userManager.GetRolesAsync(superAdmin.ApplicationUser!);
            if (roles.Any(role => role.Equals(RolesConstant.SuperAdmin, StringComparison.OrdinalIgnoreCase)))
            {
                return superAdmin.ApplicationUser;
            }
        }

        return null;
    }





    // Retrieve existing permissions for a specific user by their ID
    public async Task<AdminPermission?> GetByUserIdAsync(string userId)
    {
        return await _context.AdminPermissions
                             .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
    }

    // Add new permissions
    public async Task AddAsync(AdminPermission adminPermission)
    {
        await _context.AdminPermissions.AddAsync(adminPermission);
        await _context.SaveChangesAsync();
    }

    // Update existing permissions
    public async Task UpdateAsync(AdminPermission adminPermission)
    {
        _context.AdminPermissions.Update(adminPermission);
        await _context.SaveChangesAsync();
    }

    // Get role by name
    public async Task<AdminRole> GetRoleByNameAsync(string roleName)
    {
        var roles =  await _context.AdminRoles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        
        return roles!;        
    }

    // Add a new role to the database
    public async Task AddRoleAsync(AdminRole role)
    {
        await _context.AdminRoles.AddAsync(role);
        await _context.SaveChangesAsync(); 
    }

    
}

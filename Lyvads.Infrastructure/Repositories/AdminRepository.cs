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

    public async Task<List<AdminRole>> GetAllRolesWithPermissionsAsync()
    {
        return await _context.AdminRoles
            .Include(r => r.AdminPermissions)
            .ToListAsync();
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

    public async Task<ApplicationUser?> GetUserWithRelatedEntitiesAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.RegularUser)
            .Include(u => u.Creator)
            .Include(u => u.Admin)
            .Include(u => u.SuperAdmin)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task DeleteRelatedEntitiesAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            switch (role)
            {
                case "RegularUser":
                    if (user.RegularUser != null)
                    {
                        await DeleteRegularUserAsync(user.RegularUser.Id);
                        //_context.RegularUsers.Remove(user.RegularUser);
                    }
                    break;
                case "Creator":
                    if (user.Creator != null)
                    {
                        await DeleteCreatorAsync(user.Creator.Id);
                        //_context.Creators.Remove(user.Creator);
                    }
                    break;
                case "Admin":
                    if (user.Admin != null)
                    {
                        _context.Admins.Remove(user.Admin);
                    }
                    break;
                case "SuperAdmin":
                    if (user.SuperAdmin != null)
                    {
                        _context.SuperAdmins.Remove(user.SuperAdmin);
                    }
                    break;
            }
        }
        // Save changes to ensure related entities are removed
        await _context.SaveChangesAsync();
    }


    public async Task DeleteRegularUserAsync(string regularUserId)
    {
        // Delete Disputes
        var disputes = _context.Disputes.Where(d => d.RegularUserId == regularUserId);
        _context.Disputes.RemoveRange(disputes);

        // Delete Comments
        var comments = _context.Comments.Where(c => c.RegularUserId == regularUserId);
        _context.Comments.RemoveRange(comments);

        // Delete Follows
        var follows = _context.Follows.Where(f => f.CreatorId == regularUserId);
        _context.Follows.RemoveRange(follows);

        // Delete Requests
        var requests = _context.Requests.Where(r => r.RegularUserId == regularUserId);
        _context.Requests.RemoveRange(requests);

        // Finally, delete the RegularUser
        var regularUser = await _context.RegularUsers.FindAsync(regularUserId);
        if (regularUser != null)
        {
            _context.RegularUsers.Remove(regularUser);
        }

        // Save changes to database
        await _context.SaveChangesAsync();
    }


    public async Task DeleteCreatorAsync(string creatorId)
    {
        // Delete Disputes related to the Creator (if any)
        var disputes = _context.Disputes.Where(d => d.CreatorId == creatorId);
        _context.Disputes.RemoveRange(disputes);

        // Delete Posts related to the Creator (if any)
        var posts = _context.Posts.Where(p => p.CreatorId == creatorId);
        _context.Posts.RemoveRange(posts);

        // Delete Follows related to the Creator (if any)
        var follows = _context.Follows.Where(f => f.CreatorId == creatorId);
        _context.Follows.RemoveRange(follows);

        // Delete Requests related to the Creator (if any)
        var requests = _context.Requests.Where(r => r.CreatorId == creatorId);
        _context.Requests.RemoveRange(requests);

        // Delete Rates related to the Creator (if any) - FIX for FK constraint
        var rates = _context.Rates.Where(r => r.CreatorId == creatorId);
        _context.Rates.RemoveRange(rates);

        // Finally, delete the Creator
        var creator = await _context.Creators.FindAsync(creatorId);
        if (creator != null)
        {
            _context.Creators.Remove(creator);
        }

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

}

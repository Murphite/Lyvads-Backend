
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class AdminPermissionsRepository : IAdminPermissionsRepository
{
    private readonly AppDbContext _context;

    public AdminPermissionsRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<AdminPermission> GetByAdminUserIdAsync(string adminUserId)
    {
        var permission = await _context.Set<AdminPermission>().FirstOrDefaultAsync(p => p.ApplicationUser.Id == adminUserId);

        if (permission == null)
        {
            throw new KeyNotFoundException($"AdminPermission for user {adminUserId} was not found.");
        }

        return permission;
    }


    public async Task UpdateAsync(AdminPermission permissions)
    {
        _context.Set<AdminPermission>().Update(permissions);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(AdminRole adminRole)
    {
        await _context.Set<AdminRole>().AddAsync(adminRole);
        await _context.SaveChangesAsync();
    }



    public enum AdminRoleType
    {
        SuperAdmin,
        Admin
    }





}





using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class RegularUserRepository : IRegularUserRepository
{
    private readonly AppDbContext _context;

    public RegularUserRepository(AppDbContext context) 
    {
        _context = context;
    }

    public async Task AddAsync(RegularUser regularUser)
    {
        await _context.RegularUsers.AddAsync(regularUser);
        await _context.SaveChangesAsync();
    }

    public IQueryable<RegularUser> GetRegularUsers()
    {
        return _context.RegularUsers;
    }

    public async Task<RegularUser?> GetRegularUserByApplicationUserIdAsync(string applicationUserId)
    {
        return await _context.RegularUsers.FirstOrDefaultAsync(r => r.ApplicationUserId == applicationUserId);
    }

    public async Task<RegularUser> GetByIdWithApplicationUser(string id)
    {
        return await _context.RegularUsers
            .Include(r => r.ApplicationUser)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

}

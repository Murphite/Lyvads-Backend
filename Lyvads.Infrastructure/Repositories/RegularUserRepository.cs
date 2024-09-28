

using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;

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
}

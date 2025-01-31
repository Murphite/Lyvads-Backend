using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class CreatorRepository : ICreatorRepository
{
    private readonly AppDbContext _context;

    public CreatorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Creator> GetCreatorWithUserDetailsAsync(string userId)
    {
        // Use Include to eagerly load the ApplicationUser navigation property
        var creator = await _context.Set<Creator>()
            .Include(c => c.ApplicationUser) // Ensure the ApplicationUser navigation property is loaded
            .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

        return creator;
    }
    public async Task AddAsync(Creator creator)
    {
        await _context.Creators.AddAsync(creator);
        await _context.SaveChangesAsync();
    }

    public async Task<Creator?> GetCreatorByIdAsync(string creatorId)
    {
        // Use Include to eagerly load the ApplicationUser navigation property
        var creator = await _context.Set<Creator>()
            .Include(c => c.ApplicationUser) // Ensure the ApplicationUser navigation property is loaded
            .FirstOrDefaultAsync(c => c.ApplicationUserId == creatorId);

        return creator;
    }

    public IQueryable<Creator> GetCreators()
    {
        return _context.Creators;
    }

    public IQueryable<Creator> GetCreatorsWithDetails()
    {
        return _context.Creators.Include(c => c.Requests);
    }

    public async Task<Creator?> GetCreatorByApplicationUserIdAsync(string applicationUserId)
    {
        return await _context.Creators.FirstOrDefaultAsync(r => r.ApplicationUserId == applicationUserId);
    }

    public async Task<Creator> GetCreatorByIdWithApplicationUser(string id)
    {
        return await _context.Creators
            .Include(r => r.ApplicationUser)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}

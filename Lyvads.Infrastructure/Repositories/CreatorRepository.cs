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

    public async Task AddAsync(Creator creator)
    {
        await _context.Creators.AddAsync(creator);
        await _context.SaveChangesAsync();
    }

    public async Task<Creator?> GetCreatorByIdAsync(string creatorId)
    {
        return await _context.Creators
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == creatorId);
    }
}

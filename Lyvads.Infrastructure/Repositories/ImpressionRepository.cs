using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class ImpressionRepository : IImpressionRepository
{
    private readonly AppDbContext _context;

    public ImpressionRepository(AppDbContext context)
    {
        _context = context;
    }

    // Count total impressions
    public async Task<int> CountAsync()
    {
        return await _context.Set<Impression>().CountAsync();
    }

    // Count impressions for a specific user
    public async Task<int> CountByUserAsync(string userId)
    {
        return await _context.Set<Impression>().CountAsync(i => i.UserId == userId);
    }

    // Count impressions for a specific creator
    public async Task<int> CountByCreatorAsync(string creatorId)
    {
        return await _context.Set<Impression>().CountAsync(i => i.CreatorId == creatorId);
    }

    // Add a new impression
    public async Task AddAsync(Impression impression)
    {
        await _context.Set<Impression>().AddAsync(impression);
        await _context.SaveChangesAsync();
    }
}

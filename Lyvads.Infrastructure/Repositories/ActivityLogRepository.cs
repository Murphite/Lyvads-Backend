using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _context;

    public ActivityLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ActivityLog activityLog)
    {
        await _context.ActivityLogs.AddAsync(activityLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ActivityLog>> GetByUserIdAsync(string userId)
    {
        return await _context.ActivityLogs
            .Where(log => log.ApplicationUserId == userId)
            .ToListAsync();
    }

    public async Task<List<ActivityLog>> GetAllAsync()
    {
        return await _context.ActivityLogs.ToListAsync();
    }

}


using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog activityLog);
    Task<List<ActivityLog>> GetByUserIdAsync(string userId);
    Task<List<ActivityLog>> GetAllAsync();
}

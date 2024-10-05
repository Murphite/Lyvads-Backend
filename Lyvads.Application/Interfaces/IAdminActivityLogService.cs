
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IAdminActivityLogService
{
    Task<ServerResponse<bool>> LogActivityAsync(string userId, string userName,
        string role, string description, string category);
    Task<ServerResponse<List<ActivityLogDto>>> GetActivitiesByUserIdAsync(string userId);
    Task<ServerResponse<List<ActivityLogDto>>> GetAllActivityLogsAsync();
}

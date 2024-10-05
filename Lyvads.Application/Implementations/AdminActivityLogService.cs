using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Constants;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Implementations;

public class AdminActivityLogService : IAdminActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<AdminActivityLogService> _logger;

    public AdminActivityLogService(IActivityLogRepository activityLogRepository, ILogger<AdminActivityLogService> logger)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
    }

    public async Task<ServerResponse<bool>> LogActivityAsync(string userId, string userName,
        string role, string description, string category)
    {
        var activityLog = new ActivityLog
        {
            ApplicationUserId = userId,
            UserName = userName,
            Role = GetRoleFromString(role),
            Description = description,
            Category = category,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _activityLogRepository.AddAsync(activityLog);
        _logger.LogInformation("Logged admin activity: {Description} for user {UserName}", description, userName);

        return new ServerResponse<bool>(true)
        {
            ResponseCode = "00",
            ResponseMessage = "Admin activity logged successfully."
        };
    }

    public async Task<ServerResponse<List<ActivityLogDto>>> GetActivitiesByUserIdAsync(string userId)
    {
        var logs = await _activityLogRepository.GetByUserIdAsync(userId);
        var activityLogsDto = logs.Select(log => new ActivityLogDto
        {
            Name = log.UserName,
            Date = log.CreatedAt,
            Role = log.Role.ToString(), // Ensure this is converted to a string
            Description = log.Description,
            Category = log.Category
        }).ToList();

        return new ServerResponse<List<ActivityLogDto>>(true)
        {
            ResponseCode = "00",
            ResponseMessage = "Activities fetched successfully.",
            Data = activityLogsDto
        };
    }

    public async Task<ServerResponse<List<ActivityLogDto>>> GetAllActivityLogsAsync()
    {
        // Fetch all logs from the repository
        var logs = await _activityLogRepository.GetAllAsync();

        // Map the logs to the ActivityLogDto
        var activityLogsDto = logs.Select(log => new ActivityLogDto
        {
            Name = log.UserName,
            Date = log.CreatedAt,
            Role = log.Role.ToString(), // Ensure this is converted to a string
            Description = log.Description,
            Category = log.Category
        }).ToList();

        // Return the response
        return new ServerResponse<List<ActivityLogDto>>(true)
        {
            ResponseCode = "00",
            ResponseMessage = "All activities fetched successfully.",
            Data = activityLogsDto
        };
    }



    private string GetRoleFromString(string role)
    {
        return role switch
        {
            "CREATOR" => RolesConstant.Creator,
            "ADMIN" => RolesConstant.Admin,
            "REGULAR_USER" => RolesConstant.RegularUser,
            "SUPER_ADMIN" => RolesConstant.SuperAdmin,
            _ => throw new ArgumentException("Invalid role provided.")
        };
    }
}

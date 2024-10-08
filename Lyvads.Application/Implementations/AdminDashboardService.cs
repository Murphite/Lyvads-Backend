using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Implementations;

public class AdminDashboardService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminRepository _adminRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly IImpressionRepository _impressionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(
        UserManager<ApplicationUser> userManager,
        IAdminRepository adminRepository,
        IRequestRepository requestRepository,
        ICreatorRepository creatorRepository,
        ICollaborationRepository collaborationRepository,
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<AdminDashboardService> logger,
        IImpressionRepository impressionRepository)
    {
        _userManager = userManager;
        _adminRepository = adminRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _requestRepository = requestRepository;
        _creatorRepository = creatorRepository;
        _collaborationRepository = collaborationRepository;
        _transactionRepository = transactionRepository;
        _impressionRepository = impressionRepository;
    }


    public async Task<ServerResponse<AddUserResponseDto>> RegisterAdmin(RegisterAdminDto registerAdminDto)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var currentUser = await _userManager.FindByIdAsync(currentUserId);
        var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, RolesConstant.SuperAdmin);

        if (!isSuperAdmin)
        {
            _logger.LogWarning("Unauthorized attempt to register Admin by user: {UserId}", currentUserId);
            return new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Authorization.Error",
                    ResponseMessage = "Only SuperAdmin can register Admins"
                }
            };
        }

        // Ensure password and confirm password match
        if (registerAdminDto.Password != registerAdminDto.ConfirmPassword)
        {
            _logger.LogWarning("Password mismatch for email: {Email}", registerAdminDto.Email);
            return new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Registration.Error",
                    ResponseMessage = "Passwords do not match"
                }
            };
        }

        // Split the full name
        var names = registerAdminDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Create ApplicationUser
        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = registerAdminDto.Email,
            Email = registerAdminDto.Email,
            PhoneNumber = registerAdminDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PublicId = Guid.NewGuid().ToString(),
        };

        // Create Admin entity and associate the ApplicationUser
        var admin = new Admin
        {
            ApplicationUserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerAdminDto.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("User creation failed for email: {Email}, Errors: {Errors}", registerAdminDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Registration.Error",
                    ResponseMessage = "User creation failed",
                    ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
                }
            };
        }

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.Admin);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Role assignment failed for email: {Email}, Errors: {Errors}", registerAdminDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Role.Error",
                    ResponseMessage = "Role assignment failed",
                    ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
                }
            };
        }

        // Save the Admin entity
        await _adminRepository.AddAsync(admin);

        var addUserResponse = new AddUserResponseDto
        {
            UserId = applicationUser.Id,
            Email = applicationUser.Email,
            Role = RolesConstant.Admin,
            Message = "Admin registration successful."
        };

        _logger.LogInformation("Admin registered successfully: {Email}", registerAdminDto.Email);
        return new ServerResponse<AddUserResponseDto>
        {
            IsSuccessful = true,
            Data = addUserResponse,
            ResponseMessage = "Admin registration successful."
        };
    }

    public async Task<ServerResponse<DashboardSummaryDto>> GetDashboardSummary()
    {
        // Fetch all users
        var users = await _userManager.Users.ToListAsync();

        var totalCreatorsCount = 0;
        var totalRegularUsersCount = 0;
        var totalAdminsCount = 0;
        var totalSuperAdminsCount = 0;

        // Use a loop to check roles one by one
        foreach (var user in users)
        {
            // Check if the user is a creator
            if (await _userManager.IsInRoleAsync(user, RolesConstant.Creator))
            {
                totalCreatorsCount++;
            }

            // Check if the user is a regular user
            if (await _userManager.IsInRoleAsync(user, RolesConstant.RegularUser))
            {
                totalRegularUsersCount++;
            }
            if (await _userManager.IsInRoleAsync(user, RolesConstant.Admin))
            {
                totalAdminsCount++;
            }
            if (await _userManager.IsInRoleAsync(user, RolesConstant.SuperAdmin))
            {
                totalSuperAdminsCount++;
            }
        }

        var totalSignups = users.Count;
        var totalImpressions = await _impressionRepository.CountAsync();

        // Calculate impression percentage
        var impressionPercentage = (totalSignups > 0) ? ((double)totalImpressions / totalSignups) * 100 : 0;

        var dashboardSummary = new DashboardSummaryDto
        {
            TotalCreators = totalCreatorsCount,
            TotalRegularUsers = totalRegularUsersCount,
            TotalAdmins = totalAdminsCount,
            TotalSuperAdmins = totalSuperAdminsCount,
            TotalSignups = totalSignups,
            ImpressionPercentage = impressionPercentage
        };

        _logger.LogInformation("Dashboard summary retrieved successfully.");
        return new ServerResponse<DashboardSummaryDto>
        {
            IsSuccessful = true,
            Data = dashboardSummary
        };
    }

    public async Task<ServerResponse<RevenueReportDto>> GetRevenueReport()
    {
        var now = DateTime.UtcNow;

        var yearlyRevenue = await _transactionRepository
            .GetAllPayments()
            .Where(p => p.CreatedAt.Year == now.Year)
            .SumAsync(p => p.Amount);

        var monthlyRevenue = await _transactionRepository
            .GetAllPayments()
            .Where(p => p.CreatedAt.Year == now.Year && p.CreatedAt.Month == now.Month)
            .SumAsync(p => p.Amount);

        var weeklyRevenue = await _transactionRepository
            .GetAllPayments()
            .Where(p => p.CreatedAt >= now.AddDays(-7))
            .SumAsync(p => p.Amount);

        var dailyRevenue = await _transactionRepository
            .GetAllPayments()
            .Where(p => p.CreatedAt.Date == now.Date)
            .SumAsync(p => p.Amount);

        var revenueReport = new RevenueReportDto
        {
            YearlyRevenue = yearlyRevenue,
            MonthlyRevenue = monthlyRevenue,
            WeeklyRevenue = weeklyRevenue,
            DailyRevenue = dailyRevenue
        };

        _logger.LogInformation("Revenue report retrieved successfully.");
        return new ServerResponse<RevenueReportDto>
        {
            IsSuccessful = true,
            Data = revenueReport
        };
    }

    public async Task<ServerResponse<List<TopRequestDto>>> GetTopRequests()
    {
        var requests = await _requestRepository.GetRequests()
            .Select(r => new
            {
                r.UserId,
                r.User.ApplicationUser!.FirstName,
                r.User.ApplicationUser!.LastName,
                r.RequestType,
                TransactionAmount = r.Transactions.Sum(t => t.Amount)
            })
            .ToListAsync();

        var topRequests = requests
            .GroupBy(r => new { r.UserId, r.RequestType })
            .Select(g => new TopRequestDto
            {
                RegularUser = g.First().FirstName + " " + g.First().LastName,
                RequestType = g.Key.RequestType.ToString(),
                RequestCount = g.Count(),
                TotalAmount = g.Sum(x => x.TransactionAmount),
                TimePeriod = DateTime.UtcNow
            })
            .OrderByDescending(r => r.RequestCount)
            .ToList();

        _logger.LogInformation("Top requests retrieved successfully.");
        return new ServerResponse<List<TopRequestDto>>
        {
            IsSuccessful = true,
            Data = topRequests
        };
    }

    public async Task<ServerResponse<List<TopCreatorDto>>> GetTopCreators()
    {
        var topCreators = await _creatorRepository.GetCreators()
            .Include(c => c.Collaborations)
            .GroupBy(c => new
            {
                c.ApplicationUser.FirstName,
                c.ApplicationUser.LastName,
                c.ApplicationUser.Occupation
            })
            .Select(g => new TopCreatorDto
            {
                CreatorName = g.Key.FirstName + " " + g.Key.LastName,
                Industry = g.Key.Occupation,
                NumberOfCollaborations = g.Sum(c => c.Collaborations.Count),
                TotalAmountEarned = g.Sum(c => c.Collaborations.Sum(collab => collab.Amount)),
            })
            .OrderByDescending(c => c.NumberOfCollaborations)
            .ToListAsync();

        _logger.LogInformation("Top creators retrieved successfully.");
        return new ServerResponse<List<TopCreatorDto>>
        {
            IsSuccessful = true,
            Data = topCreators
        };
    }

    public async Task<ServerResponse<CollaborationStatusReportDto>> GetCollaborationStatusesReport()
    {
        var now = DateTime.UtcNow;

        var successfulCollaborations = await _collaborationRepository.GetAllCollaborations()
            .Where(c => c.Status == CollaborationStatus.Completed && c.CreatedAt.Year == now.Year)
            .CountAsync();

        var pendingCollaborations = await _collaborationRepository.GetAllCollaborations()
            .Where(c => c.Status == CollaborationStatus.Pending && c.CreatedAt.Year == now.Year)
            .CountAsync();

        var declinedCollaborations = await _collaborationRepository.GetAllCollaborations()
            .Where(c => c.Status == CollaborationStatus.Declined && c.CreatedAt.Year == now.Year)
            .CountAsync();

        var collaborationStatusReport = new CollaborationStatusReportDto
        {
            SuccessfulCollaborations = successfulCollaborations,
            PendingCollaborations = pendingCollaborations,
            DeclinedCollaborations = declinedCollaborations
        };

        _logger.LogInformation("Collaboration statuses report retrieved successfully.");
        return new ServerResponse<CollaborationStatusReportDto>
        {
            IsSuccessful = true,
            Data = collaborationStatusReport
        };
    }


}

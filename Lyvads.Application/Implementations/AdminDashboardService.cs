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

        // Check if the current user exists
        if (currentUser == null)
        {
            _logger.LogWarning("User not found: {UserId}", currentUserId);
            return new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Authorization.Error",
                    ResponseMessage = "Current user not found"
                }
            };
        }

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

    public async Task<ServerResponse<TPRevenueReportDto>> GetRevenueReport(string timePeriod)
    {
        var now = DateTime.UtcNow;
        decimal totalRevenue;

        switch (timePeriod.ToLower())
        {
            case "yearly":
                totalRevenue = await _transactionRepository
                    .GetAllPayments()
                    .Where(p => p.CreatedAt.Year == now.Year)
                    .SumAsync(p => p.Amount);
                break;

            case "monthly":
                totalRevenue = await _transactionRepository
                    .GetAllPayments()
                    .Where(p => p.CreatedAt.Year == now.Year && p.CreatedAt.Month == now.Month)
                    .SumAsync(p => p.Amount);
                break;

            case "weekly":
                totalRevenue = await _transactionRepository
                    .GetAllPayments()
                    .Where(p => p.CreatedAt >= now.AddDays(-7))
                    .SumAsync(p => p.Amount);
                break;

            case "daily":
                totalRevenue = await _transactionRepository
                    .GetAllPayments()
                    .Where(p => p.CreatedAt.Date == now.Date)
                    .SumAsync(p => p.Amount);
                break;

            default:
                _logger.LogWarning("Invalid time period specified: {TimePeriod}", timePeriod);
                return new ServerResponse<TPRevenueReportDto>
                {
                    IsSuccessful = false,
                    ResponseMessage = "Invalid time period. Please specify 'yearly', 'monthly', 'weekly', or 'daily'."
                };
        }

        var revenueReport = new TPRevenueReportDto
        {
            TotalRevenue = totalRevenue,
            TimePeriod = timePeriod
        };

        _logger.LogInformation("{TimePeriod} revenue report retrieved successfully.", timePeriod);
        return new ServerResponse<TPRevenueReportDto>
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
                r.RegularUserId,
                r.RegularUser!.ApplicationUser!.FirstName,
                r.RegularUser.ApplicationUser!.LastName,
                r.RegularUser.ApplicationUser.ImageUrl,
                r.RequestType,
                TransactionAmount = r.Transactions.Sum(t => t.Amount)
            })
            .ToListAsync();

        var topRequests = requests
            .GroupBy(r => new { r.RegularUserId, r.RequestType })
            .Select(g => new TopRequestDto
            {
                RegularUser = g.First().FirstName + " " + g.First().LastName,
                RequestType = g.Key.RequestType.ToString(),
                ProfilePic = g.First().ImageUrl,
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
        var creatorsData = await _creatorRepository.GetCreators()
            .Include(c => c.Collaborations)
            .Select(c => new
            {
                CreatorId = c.Id,
                CreatorName = c.ApplicationUser!.FirstName + " " + c.ApplicationUser.LastName,
                ProfilePic = c.ApplicationUser.ImageUrl,
                Industry = c.ApplicationUser.Occupation,
                NumberOfCollaborations = c.Collaborations.Count,
                TotalAmountEarned = c.Collaborations.Sum(collab => collab.Amount)
            })
            .ToListAsync();

        var topCreators = creatorsData
            .GroupBy(c => new { c.CreatorId, c.CreatorName, c.ProfilePic, c.Industry })
            .Select(g => new TopCreatorDto
            {
                CreatorId = g.Key.CreatorId,
                CreatorName = g.Key.CreatorName,
                ProfilePic = g.Key.ProfilePic,
                Industry = g.Key.Industry,
                NumberOfCollaborations = g.Sum(c => c.NumberOfCollaborations),
                TotalAmountEarned = g.Sum(c => c.TotalAmountEarned)
            })
            .OrderByDescending(c => c.NumberOfCollaborations)
            .ToList();

        _logger.LogInformation("Top creators retrieved successfully.");
        return new ServerResponse<List<TopCreatorDto>>
        {
            IsSuccessful = true,
            Data = topCreators
        };
    }


    public async Task<ServerResponse<List<CollaborationStatusDto>>> GetCollaborationStatusesReport()
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

        var collaborationStatusReport = new List<CollaborationStatusDto>
    {
        new CollaborationStatusDto { Name = "SuccessfulCollaborations", Value = successfulCollaborations },
        new CollaborationStatusDto { Name = "PendingCollaborations", Value = pendingCollaborations },
        new CollaborationStatusDto { Name = "DeclinedCollaborations", Value = declinedCollaborations }
    };

        _logger.LogInformation("Collaboration statuses report retrieved successfully.");
        return new ServerResponse<List<CollaborationStatusDto>>
        {
            IsSuccessful = true,
            Data = collaborationStatusReport
        };
    }

    public async Task<ServerResponse<MonthlyRevenueReportDto>> GetMonthlyRevenueReportAsync(int year)
    {
        var monthlyRevenueData = await _transactionRepository
            .GetAllPayments()
            .Where(p => p.CreatedAt.Year == year)
            .GroupBy(p => p.CreatedAt.Month)
            .Select(g => new MonthlyRevenueDto
            {
                Month = g.Key,
                TotalRevenue = g.Sum(p => p.Amount)
            })
            .ToListAsync();

        // Ensure all months are included, with 0 revenue for months with no transactions
        var fullMonthlyRevenue = Enumerable.Range(1, 12)
            .Select(month => new MonthlyRevenueDto
            {
                Month = month,
                TotalRevenue = monthlyRevenueData.FirstOrDefault(m => m.Month == month)?.TotalRevenue ?? 0
            })
            .ToList();

        // Calculate Grand Total Revenue for the year
        var grandTotalRevenue = fullMonthlyRevenue.Sum(m => m.TotalRevenue);

        var revenueReport = new MonthlyRevenueReportDto
        {
            Year = year,
            MonthlyRevenue = fullMonthlyRevenue,
            GrandTotalRevenue = grandTotalRevenue
        };

        _logger.LogInformation("Monthly revenue report for year {Year} retrieved successfully.", year);
        return new ServerResponse<MonthlyRevenueReportDto>
        {
            IsSuccessful = true,
            Data = revenueReport
        };
    }

}

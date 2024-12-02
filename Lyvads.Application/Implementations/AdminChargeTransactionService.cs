using Lyvads.Domain.Repositories;
using Lyvads.Domain.Entities;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Responses;
using Microsoft.Extensions.Logging;
using Lyvads.Domain.Enums;
using Lyvads.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Lyvads.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Lyvads.Application.Implementations;

public class AdminChargeTransactionService : IAdminChargeTransactionService
{
    private readonly ILogger<AdminChargeTransactionService> _logger;
    private readonly IChargeTransactionRepository _chargeTransactionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAdminActivityLogService _adminActivityLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminChargeTransactionService(ILogger<AdminChargeTransactionService> logger,
        IChargeTransactionRepository chargeTransactionRepository,
        IHttpContextAccessor httpContextAccessor,
        IAdminActivityLogService adminActivityLogService,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _chargeTransactionRepository = chargeTransactionRepository;
        _httpContextAccessor = httpContextAccessor;
        _adminActivityLogService = adminActivityLogService;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<ServerResponse<ChargeSummaryDto>> GetChargeSummaryAsync()
    {
        try
        {
            // Fetch all charge transactions from the database
            var chargeTransactions = await _chargeTransactionRepository.GetAllAsync();

            // Calculate the total sum of all charges
            var totalCharges = chargeTransactions.Sum(ct => ct.Amount);

            // Calculate the total sum for each charge type
            var chargeTypeTotals = chargeTransactions
                .GroupBy(ct => ct.ChargeName)
                .Select(group => new ChargeTypeTotal
                {
                    ChargeName = group.Key,
                    TotalAmount = group.Sum(ct => ct.Amount)
                })
                .ToList();

            // Prepare the response DTO
            var result = new ChargeSummaryDto
            {
                TotalCharges = totalCharges,
                ChargeTypeTotals = chargeTypeTotals
            };

            return new ServerResponse<ChargeSummaryDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating charge summary");
            return new ServerResponse<ChargeSummaryDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    // Get all ChargeTransactions
    public async Task<ServerResponse<List<ChargeTransactionDto>>> GetAllChargeTransactionsAsync()
    {
        try
        {
            var chargeTransactions = await _chargeTransactionRepository.GetAllAsync();

            var result = chargeTransactions.Select(ct => new ChargeTransactionDto
            {
                Id = ct.Id,
                UserName = ct.ApplicationUser.FullName,
                ChargeName = ct.ChargeName,
                Amount = ct.Amount,
                DateCharged = ct.CreatedAt,
                Status = ct.Status.ToString(),
            }).ToList();

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole!,
                    "Got All Charge Transactions",
                    "Charge Management");
            }

            return new ServerResponse<List<ChargeTransactionDto>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching charge transactions");
            return new ServerResponse<List<ChargeTransactionDto>>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    // Add new Charge
    public async Task<ServerResponse<CreateChargeResponse>> AddNewChargeAsync(CreateChargeDto chargeDto)
    {
        try
        {
            // Create the new charge
            var charge = new Charge
            {
                ChargeName = chargeDto.ChargeName,
                Percentage = chargeDto.Percentage,
                MinAmount = chargeDto.MinAmount,
                MaxAmount = chargeDto.MaxAmount,
                Status = ChargeStatus.Active
            };

            // Add the charge to the repository
            await _chargeTransactionRepository.AddAsync(charge);

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            // Check if the current user is in any required role
            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            if (userRole == null)
            {
                _logger.LogWarning("User does not have the required roles.");
                return new ServerResponse<CreateChargeResponse>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "403",
                        ResponseMessage = "Forbidden",
                        ResponseDescription = "User does not have permission to add a charge."
                    }
                };
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole,
                    $"Added new charge: {chargeDto.ChargeName}",
                    "Charge Management");
            }

            // Prepare the response with the charge ID
            var response = new CreateChargeResponse
            {
                Id = charge.Id, // Assuming the Charge entity has an Id property
                ChargeName = chargeDto.ChargeName,
                Percentage = chargeDto.Percentage,
                MinAmount = chargeDto.MinAmount,
                MaxAmount = chargeDto.MaxAmount
            };

            return new ServerResponse<CreateChargeResponse>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Charge added successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new charge");
            return new ServerResponse<CreateChargeResponse>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    // Edit Charge by Id
    public async Task<ServerResponse<EditChargeResponse>> EditChargeAsync(string chargeId, EditChargeDto chargeDto)
    {
        try
        {
            var charge = await _chargeTransactionRepository.GetChargeByIdAsync(chargeId);
            if (charge == null)
            {
                return new ServerResponse<EditChargeResponse>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Charge not found",
                        ResponseDescription = "The charge with the given ID does not exist"
                    }
                };
            }

            charge.ChargeName = chargeDto.ChargeName;
            charge.Percentage = chargeDto.Percentage;
            charge.MinAmount = chargeDto.MinAmount;
            charge.MaxAmount = chargeDto.MaxAmount;
            charge.Status = chargeDto.Status;

            await _chargeTransactionRepository.UpdateAsync(charge);

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole!,
                    $"Edited Charge Details: {chargeId}",
                    "Charge Management");
            }

            var response = new EditChargeResponse
            {
                Id = charge.Id, // Assuming the Charge entity has an Id property
                ChargeName = chargeDto.ChargeName,
                Percentage = chargeDto.Percentage,
                MinAmount = chargeDto.MinAmount,
                MaxAmount = chargeDto.MaxAmount,
                Status = chargeDto.Status.ToString()
            };

            return new ServerResponse<EditChargeResponse>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Charge updated successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating charge");
            return new ServerResponse<EditChargeResponse>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

   
    // Get ChargeTransaction by Id
    public async Task<ServerResponse<ChargeTransactionDto>> GetChargeTransactionByIdAsync(string chargeTransactionId)
    {
        try
        {
            var chargeTransaction = await _chargeTransactionRepository.GetByCTIdAsync(chargeTransactionId);
            if (chargeTransaction == null)
            {
                return new ServerResponse<ChargeTransactionDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Charge transaction not found",
                        ResponseDescription = "The charge transaction with the given ID does not exist"
                    }
                };
            }

            var result = new ChargeTransactionDto
            {
                Id = chargeTransactionId,
                UserName = chargeTransaction.ApplicationUser.FullName,
                ChargeName = chargeTransaction.ChargeName,
                Amount = chargeTransaction.Amount,
                DateCharged = chargeTransaction.CreatedAt,
                Status = chargeTransaction.Status.ToString(),
            };

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole!,
                    "Got Charge Transaction Details By ID",
                    "Charge Management");
            }

            return new ServerResponse<ChargeTransactionDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching charge transaction by ID");
            return new ServerResponse<ChargeTransactionDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

   
    // Delete Charge
    public async Task<ServerResponse<string>> DeleteChargeAsync(string chargeId)
    {
        try
        {
            var charge = await _chargeTransactionRepository.GetChargeByIdAsync(chargeId);
            if (charge == null)
            {
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Charge not found",
                        ResponseDescription = "The charge with the given ID does not exist"
                    }
                };
            }

            await _chargeTransactionRepository.DeleteAsync(charge);

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole!,
                    $"Deleted Charge: {chargeId}",
                    "Charge Management");
            }

            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Charge deleted successfully",
                Data = "Charge has been removed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting charge");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    // Get all Charges
    public async Task<ServerResponse<List<ChargeDto>>> GetAllChargesAsync()
    {
        try
        {
            var charges = await _chargeTransactionRepository.GetAllChargesAsync();

            var result = charges.Select(c => new ChargeDto
            {
                Id = c.Id,
                ChargeName = c.ChargeName,
                Percentage = c.Percentage,
                MinAmount = c.MinAmount,
                MaxAmount = c.MaxAmount,
                Status = c.Status.ToString(),
            }).ToList();

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole!,
                    "Got All Charges",
                    "Charge Management");
            }

            return new ServerResponse<List<ChargeDto>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all charges");
            return new ServerResponse<List<ChargeDto>>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    
    // Get Charge by Id
    public async Task<ServerResponse<ChargeDto>> GetChargeByIdAsync(string chargeId)
    {
        try
        {
            var charge = await _chargeTransactionRepository.GetChargeByIdAsync(chargeId);
            if (charge == null)
            {
                return new ServerResponse<ChargeDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Charge not found",
                        ResponseDescription = "The charge with the given ID does not exist"
                    }
                };
            }

            var result = new ChargeDto
            {
                Id = chargeId,
                ChargeName = charge.ChargeName,
                Percentage = charge.Percentage,
                MinAmount = charge.MinAmount,
                MaxAmount = charge.MaxAmount,
                Status = charge.Status.ToString()
            };

            // Get the currently logged-in admin user's ID
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Use current user service to fetch the current admin username
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var rolesToCheck = new[] { RolesConstant.SuperAdmin, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.Creator };
            string? userRole = null;

            foreach (var role in rolesToCheck)
            {
                if (await _userManager.IsInRoleAsync(currentUser, role))
                {
                    userRole = role;
                    break;
                }
            }

            // Ensure user ID and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || currentUser == null)
            {
                _logger.LogWarning("User ID or current user is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    currentUser.FullName!,
                    userRole!,
                    "Got Charge By ID",
                    "Charge Management");
            }

            return new ServerResponse<ChargeDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Success",
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching charge by ID");
            return new ServerResponse<ChargeDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }
}

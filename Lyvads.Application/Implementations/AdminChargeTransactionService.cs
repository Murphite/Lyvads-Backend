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

namespace Lyvads.Application.Implementations;

public class AdminChargeTransactionService : IAdminChargeTransactionService
{
    private readonly ILogger<AdminChargeTransactionService> _logger;
    private readonly IChargeTransactionRepository _chargeTransactionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAdminActivityLogService _adminActivityLogService;

    public AdminChargeTransactionService(ILogger<AdminChargeTransactionService> logger,
        IChargeTransactionRepository chargeTransactionRepository,
        IHttpContextAccessor httpContextAccessor,
        IAdminActivityLogService adminActivityLogService)
    {
        _logger = logger;
        _chargeTransactionRepository = chargeTransactionRepository;
        _httpContextAccessor = httpContextAccessor;
        _adminActivityLogService = adminActivityLogService;
    }

    // Get all ChargeTransactions
    public async Task<ServerResponse<List<ChargeTransactionDto>>> GetAllChargeTransactionsAsync()
    {
        try
        {
            var chargeTransactions = await _chargeTransactionRepository.GetAllAsync();

            var result = chargeTransactions.Select(ct => new ChargeTransactionDto
            {
                UserName = ct.UserName,
                ChargeName = ct.ChargeName,
                Amount = ct.Amount,
                DateCharged = ct.CreatedAt,
                Status = ct.Status
            }).ToList();

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
    public async Task<ServerResponse<string>> AddNewChargeAsync(CreateChargeDto chargeDto)
    {
        try
        {
            var charge = new Charge
            {
                ChargeName = chargeDto.ChargeName,
                Percentage = chargeDto.Percentage,
                MinAmount = chargeDto.MinAmount,
                MaxAmount = chargeDto.MaxAmount,
                Status = ChargeStatus.Active
            };

            await _chargeTransactionRepository.AddAsync(charge);

            // Get the currently logged-in admin user's ID and username
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

            // Ensure userId and username are not null before proceeding
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("User ID or Username is null. Activity log will not be recorded.");
            }
            else
            {
                // Log the admin activity
                await _adminActivityLogService.LogActivityAsync(
                    userId,
                    username,
                    RolesConstant.Admin,
                    "Added new charge: " + chargeDto.ChargeName,
                    "Charge Management");
            }

            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Charge added successfully",
                Data = "Charge has been created"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new charge");
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

    // Edit Charge by Id
    public async Task<ServerResponse<string>> EditChargeAsync(string chargeId, EditChargeDto chargeDto)
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

            charge.ChargeName = chargeDto.ChargeName;
            charge.Percentage = chargeDto.Percentage;
            charge.MinAmount = chargeDto.MinAmount;
            charge.MaxAmount = chargeDto.MaxAmount;
            charge.Status = chargeDto.Status;

            await _chargeTransactionRepository.UpdateAsync(charge);

            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Charge updated successfully",
                Data = "Charge has been updated"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating charge");
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
                UserName = chargeTransaction.UserName,
                ChargeName = chargeTransaction.ChargeName,
                Amount = chargeTransaction.Amount,
                DateCharged = chargeTransaction.CreatedAt,
                Status = chargeTransaction.Status
            };

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
                ChargeName = c.ChargeName,
                Percentage = c.Percentage,
                MinAmount = c.MinAmount,
                MaxAmount = c.MaxAmount,
                Status = c.Status
            }).ToList();

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
                ChargeName = charge.ChargeName,
                Percentage = charge.Percentage,
                MinAmount = charge.MinAmount,
                MaxAmount = charge.MaxAmount,
                Status = charge.Status
            };

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

using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementations;

public class AdminUserAdService : IUserAdService
{
    private readonly IUserAdRepository _userAdRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminUserAdService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserAdService(
        IUserAdRepository userAdRepository,
        ILogger<AdminUserAdService> logger,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _userAdRepository = userAdRepository;
        _logger = logger;
        _userManager = userManager;
        _currentUserService = currentUserService;
    }


    public async Task<ServerResponse<AddUserAdResponseDto>> AddUserAdAsync(AddUserAdDto userAdDto)
    {
        
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
            {
                _logger.LogWarning("Current user not found: {UserId}", currentUserId);
                return new ServerResponse<AddUserAdResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "User not found",
                        ResponseDescription = "The current user does not exist."
                    }
                };
            }

            var userAd = new UserAd
            {
                ApplicationUserId = currentUser.Id,
                Email = currentUser.Email!,
                FullName = currentUser.FullName!,
                Description = userAdDto.Description!,
                Amount = userAdDto.Amount,
                CreatedAt = DateTime.UtcNow,
                Status = UserAdStatus.Pending
            };

            await _userAdRepository.AddUserAdAsync(userAd);

            var responseDto = new AddUserAdResponseDto
            {
                UserId = userAd.Id,
                Status = userAd.Status.ToString(),
                CreatedAt = userAd.CreatedAt,
                Email = userAd.Email!,
                FullName = userAd.FullName!,
                Description = userAd.Description!,
                Amount = userAd.Amount,
            };

        return new ServerResponse<AddUserAdResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "User ad added successfully.",
                Data = responseDto
        };
        
       
    }


    public async Task<ServerResponse<EditUserAdResponseDto>> EditUserAdAsync(string adId, EditUserAdDto editUserAdDto)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
            {
                _logger.LogWarning("Current user not found: {UserId}", currentUserId);
                return new ServerResponse<EditUserAdResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "User not found",
                        ResponseDescription = "The current user does not exist."
                    }
                };
            }

            var userAd = await _userAdRepository.GetUserAdByIdAsync(adId);

            if (userAd == null || userAd.ApplicationUserId != currentUserId)
            {
                _logger.LogWarning("User ad not found or unauthorized access. AdId: {AdId}, UserId: {UserId}", adId, currentUserId);
                return new ServerResponse<EditUserAdResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Ad not found",
                        ResponseDescription = "The specified user ad does not exist or you do not have permission to edit it."
                    }
                };
            }

            // Update user ad details
            userAd.Description = editUserAdDto.Description ?? userAd.Description;
            userAd.Amount = editUserAdDto.Amount ?? userAd.Amount;
            userAd.UpdatedAt = DateTime.UtcNow;

            await _userAdRepository.UpdateUserAdAsync(userAd);

            var responseDto = new EditUserAdResponseDto
            {
                UserId = userAd.ApplicationUserId,
                AdId = userAd.Id,
                Description = userAd.Description,
                Amount = userAd.Amount,
                Status = userAd.Status.ToString(),
                UpdatedAt = userAd.UpdatedAt,
            };

            _logger.LogInformation("User ad updated successfully. AdId: {AdId}, UserId: {UserId}", adId, currentUserId);
            return new ServerResponse<EditUserAdResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "User ad updated successfully.",
                Data = responseDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing user ad. AdId: {AdId}, UserId: {UserId}", adId, _currentUserService.GetCurrentUserId());
            return new ServerResponse<EditUserAdResponseDto>
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


    public async Task<ServerResponse<List<UserAdDto>>> GetAllUserAdsAsync()
    {
        try
        {
            // Get raw data from the repository
            var ads = await _userAdRepository.GetAllAsync();

            // Convert raw ads to UserAdDto
            var adDtos = ads.Select(ad => new UserAdDto
            {
                Id = ad.Id,
                UserName = ad.ApplicationUser.FirstName + " " + ad.ApplicationUser.LastName,
                Description = ad.Description,
                Email = ad.Email,
                Amount = ad.Amount,
                DateCreated = ad.CreatedAt,
                Status = ad.Status.ToString()
            }).ToList();

            // Wrap in ServerResponse
            return new ServerResponse<List<UserAdDto>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Success",
                Data = adDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all user ads");
            return new ServerResponse<List<UserAdDto>>
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

    public async Task<ServerResponse<string>> ToggleAdStatusAsync(string adId)
    {
        try
        {
            // Fetch the ad by its ID
            var ad = await _userAdRepository.GetByIdAsync(adId);
            if (ad == null)
            {
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Ad not found"
                    }
                };
            }

            // Toggle the ad's status
            if (ad.Status == UserAdStatus.Approved)
            {
                // If ad is approved, decline it
                ad.Status = UserAdStatus.Declined;
                _logger.LogInformation($"Ad with ID {adId} has been declined.");
            }
            else if (ad.Status == UserAdStatus.Declined)
            {
                // If ad is declined, approve it
                ad.Status = UserAdStatus.Approved;
                _logger.LogInformation($"Ad with ID {adId} has been approved.");
            }
            else
            {
                // If the ad is in any other state, consider it neutral and cannot toggle
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Ad cannot be toggled because its current status is neither approved nor declined."
                };
            }

            // Update the ad status in the repository
            await _userAdRepository.UpdateAsync(ad);

            // Return a successful response
            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = ad.Status == UserAdStatus.Approved ? "Ad approved successfully" : "Ad declined successfully"
            };
        }
        catch (Exception ex)
        {
            // Log and handle any errors
            _logger.LogError(ex, "Error toggling ad status for Ad ID: {AdId}", adId);
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


    public async Task<ServerResponse<string>> ApproveAdAsync(string adId)
    {
        try
        {
            // Fetch the ad by its ID
            var ad = await _userAdRepository.GetByIdAsync(adId);
            if (ad == null)
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Ad not found"
                    }
                };

            ad.Status = UserAdStatus.Approved;
            await _userAdRepository.UpdateAsync(ad);

            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Ad approved successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving ad with ID: {AdId}", adId);
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

    public async Task<ServerResponse<string>> DeclineAdAsync(string adId)
    {
        try
        {
            // Fetch the ad by its ID
            var ad = await _userAdRepository.GetByIdAsync(adId);
            if (ad == null)
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Ad not found"
                    }
                };

            ad.Status = UserAdStatus.Declined;
            await _userAdRepository.UpdateAsync(ad);

            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Ad declined successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining ad with ID: {AdId}", adId);
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

}

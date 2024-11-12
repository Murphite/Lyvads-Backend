using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementations;

public class AdminUserAdService : IUserAdService
{
    private readonly IUserAdRepository _userAdRepository;
    private readonly ILogger<AdminUserAdService> _logger;

    public AdminUserAdService(IUserAdRepository userAdRepository,
        ILogger<AdminUserAdService> logger)
    {
        _userAdRepository = userAdRepository;
        _logger = logger;
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
                Amount = ad.Amount,
                DateCreated = ad.CreatedAt,
                Status = ad.Status
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

}

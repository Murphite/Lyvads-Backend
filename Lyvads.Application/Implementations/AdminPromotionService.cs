

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Entities;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Implementations;

public class AdminPromotionService : IPromotionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminRepository _adminRepository;
    private readonly IMediaService _mediaService;
    private readonly ICreatorRepository _creatorRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IImpressionRepository _impressionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminPromotionService(
        UserManager<ApplicationUser> userManager,
        IAdminRepository adminRepository,
        IMediaService mediaService,
        ICreatorRepository creatorRepository,
        IPromotionRepository promotionRepository,
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        ILogger<AdminDashboardService> logger,
        IImpressionRepository impressionRepository)
    {
        _userManager = userManager;
        _adminRepository = adminRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _mediaService = mediaService;
        _creatorRepository = creatorRepository;
        _promotionRepository = promotionRepository;
        _transactionRepository = transactionRepository;
        _impressionRepository = impressionRepository;
    }

    // Assuming these namespaces are already imported
    public async Task<ServerResponse<PromotionDto>> AddPromotion(CreatePromotionDto createPromotionDto)
    {
        // Define a folder name for the media upload
        string folderName = "promotions";

        try
        {
            // Determine the media type
            var mediaType = createPromotionDto.Media!.ContentType.ToLower();
            Dictionary<string, string> mediaResponse;

            if (mediaType.Contains("image"))
            {
                // Upload image
                mediaResponse = await _mediaService.UploadImageAsync(createPromotionDto.Media, folderName);
            }
            else if (mediaType.Contains("video"))
            {
                // Upload video
                mediaResponse = await _mediaService.UploadVideoAsync(createPromotionDto.Media, folderName);
            }
            else
            {
                // Handle unsupported media type
                return new ServerResponse<PromotionDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Unsupported media type.",
                        ResponseDescription = "The provided media type is not supported."
                    }
                };
            }

            // Check if the upload was successful
            if (mediaResponse["Code"] != "200")
            {
                return new ServerResponse<PromotionDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = mediaResponse["Code"],
                        ResponseMessage = "Media upload failed.",
                        ResponseDescription = mediaResponse["Message"]
                    }
                };
            }

            // Extract the media URL from the response
            var mediaUrl = mediaResponse["Url"];

            var promotion = new Promotion
            {
                Title = createPromotionDto.Title,
                ShortDescription = createPromotionDto.ShortDescription,
                Price = createPromotionDto.Price,
                MediaUrl = mediaUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _promotionRepository.AddAsync(promotion);

            var promotionDto = new PromotionDto
            {
                Id = promotion.Id,
                Title = promotion.Title,
                ShortDescription = promotion.ShortDescription,
                Price = promotion.Price,
                MediaUrl = promotion.MediaUrl,
                IsHidden = promotion.IsHidden,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt
            };

            return new ServerResponse<PromotionDto>
            {
                IsSuccessful = true,
                Data = promotionDto,
                ResponseMessage = "Promotion added successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding promotion");
            return new ServerResponse<PromotionDto>
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

    public async Task<ServerResponse<PromotionDto>> UpdatePromotion(string promotionId, UpdatePromotionDto updatePromotionDto)
    {
        // Define a folder name for the media upload
        string folderName = "promotions";

        try
        {
            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            if (promotion == null)
            {
                return new ServerResponse<PromotionDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Promotion not found",
                        ResponseDescription = $"Promotion with ID {promotionId} does not exist."
                    }
                };
            }

            // Determine the media type
            var mediaType = updatePromotionDto.Media!.ContentType.ToLower();
            Dictionary<string, string> mediaResponse;

            if (mediaType.Contains("image"))
            {
                // Upload image
                mediaResponse = await _mediaService.UploadImageAsync(updatePromotionDto.Media, folderName);
            }
            else if (mediaType.Contains("video"))
            {
                // Upload video
                mediaResponse = await _mediaService.UploadVideoAsync(updatePromotionDto.Media, folderName);
            }
            else
            {
                // Handle unsupported media type
                return new ServerResponse<PromotionDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Unsupported media type.",
                        ResponseDescription = "The provided media type is not supported."
                    }
                };
            }

            // Check if the upload was successful
            if (mediaResponse["Code"] != "200")
            {
                return new ServerResponse<PromotionDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = mediaResponse["Code"],
                        ResponseMessage = "Media upload failed.",
                        ResponseDescription = mediaResponse["Message"]
                    }
                };
            }

            // Extract the media URL from the response
            var mediaUrl = mediaResponse["Url"];

            promotion.Title = updatePromotionDto.Title;
            promotion.ShortDescription = updatePromotionDto.ShortDescription;
            promotion.Price = updatePromotionDto.Price;
            promotion.MediaUrl = mediaUrl; // Update the media URL
            promotion.UpdatedAt = DateTime.UtcNow;

            await _promotionRepository.UpdateAsync(promotion);

            var promotionDto = new PromotionDto
            {
                Id = promotion.Id,
                Title = promotion.Title,
                ShortDescription = promotion.ShortDescription,
                Price = promotion.Price,
                MediaUrl = promotion.MediaUrl,
                IsHidden = promotion.IsHidden,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt
            };

            return new ServerResponse<PromotionDto>
            {
                IsSuccessful = true,
                Data = promotionDto,
                ResponseMessage = "Promotion updated successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating promotion with ID {PromotionId}", promotionId);
            return new ServerResponse<PromotionDto>
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

    public async Task<ServerResponse<object>> DeletePromotion(string promotionId)
    {
        try
        {
            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            if (promotion == null)
            {
                return new ServerResponse<object>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Promotion not found",
                        ResponseDescription = $"Promotion with ID {promotionId} does not exist."
                    }
                };
            }

            await _promotionRepository.DeleteAsync(promotion);
            return new ServerResponse<object>
            {
                IsSuccessful = true,
                ResponseMessage = "Promotion deleted successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting promotion with ID {PromotionId}", promotionId);
            return new ServerResponse<object>
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

    public async Task<ServerResponse<object>> TogglePromotionVisibility(string promotionId, bool hide)
    {
        try
        {
            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            if (promotion == null)
            {
                return new ServerResponse<object>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "Promotion not found",
                        ResponseDescription = $"Promotion with ID {promotionId} does not exist."
                    }
                };
            }

            promotion.IsHidden = hide;
            promotion.UpdatedAt = DateTime.UtcNow;

            await _promotionRepository.UpdateAsync(promotion);
            return new ServerResponse<object>
            {
                IsSuccessful = true,
                ResponseMessage = hide ? "Promotion hidden" : "Promotion unhidden"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling promotion visibility for ID {PromotionId}", promotionId);
            return new ServerResponse<object>
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

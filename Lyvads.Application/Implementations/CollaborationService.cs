using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Lyvads.Application.Dtos.CreatorDtos;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using Lyvads.Domain.Constants;


namespace Lyvads.Application.Implementations;

public class CollaborationService : ICollaborationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICollaborationRepository _collaborationRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly IRepository _repository;
    private readonly IRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDisputeRepository _disputeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminService> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IMediaService _mediaService;
    private readonly IWalletService _walletService;

    public CollaborationService(
        UserManager<ApplicationUser> userManager,
        ICollaborationRepository collaborationRepository,
        ICurrentUserService currentUserService,
        ICreatorRepository creatorRepository,
        IRepository repository,
        IUserRepository userRepository,
        IDisputeRepository disputeRepository,
        IRequestRepository requestRepository,
        ILogger<AdminService> logger, 
        IMediaService mediaService, 
        IWalletService walletService,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger;
        _collaborationRepository = collaborationRepository;
        _repository = repository;
        _userRepository = userRepository;
        _disputeRepository = disputeRepository;
        _creatorRepository = creatorRepository;
        _requestRepository = requestRepository;
        _webHostEnvironment = webHostEnvironment;
        _mediaService = mediaService;
        _walletService = walletService;
    }

    public async Task<ServerResponse<List<CollaborationDto>>> GetCollaborationsAsync()
    {
        _logger.LogInformation("Fetching all collaborations...");

        try
        {
            var collaborations = await _collaborationRepository.GetAllAsync();

            if (collaborations == null || !collaborations.Any())
            {
                _logger.LogWarning("No collaborations found.");
                return new ServerResponse<List<CollaborationDto>>(false)
                {
                    ResponseCode = "404",
                    ResponseMessage = "No collaborations found.",
                };
            }

            var collaborationDtos = collaborations.Select(c => new CollaborationDto
            {
                Id = c.Id,
                RegularUserName = c.RegularUser.ApplicationUser.FirstName + " " + c.RegularUser.ApplicationUser.LastName,
                CreatorName = c.Creator.ApplicationUser.FirstName + " " + c.Creator.ApplicationUser.LastName,
                Amount = c.Amount,
                RequestDate = c.CreatedAt,
                Status = c.Status,
                ReceiptUrl = c.ReceiptUrl
            }).ToList();

            _logger.LogInformation("Successfully fetched {Count} collaborations.", collaborationDtos.Count);
            return new ServerResponse<List<CollaborationDto>>(true)
            {
                ResponseCode = "00",
                ResponseMessage = "Collaborations fetched successfully.",
                Data = collaborationDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching collaborations.");
            return new ServerResponse<List<CollaborationDto>>(false)
            {
                ResponseCode = "500",
                ResponseMessage = "An error occurred while fetching collaborations."
            };
        }
    }

    public async Task<ServerResponse<CollaborationDto>> GetCollaborationDetailsAsync(string collaborationId)
    {
        _logger.LogInformation("Fetching details for collaboration ID: {CollaborationId}", collaborationId);

        try
        {
            var collaboration = await _collaborationRepository.GetByIdAsync(collaborationId);
            if (collaboration == null)
            {
                _logger.LogWarning("Collaboration not found with ID: {CollaborationId}", collaborationId);
                return new ServerResponse<CollaborationDto>(false)
                {
                    ResponseCode = "404",
                    ResponseMessage = "Collaboration not found."
                };
            }

            var collaborationDto = new CollaborationDto
            {
                Id = collaboration.Id,
                RegularUserName = collaboration.RegularUser.ApplicationUser.FirstName + " " + collaboration.RegularUser.ApplicationUser.LastName,
                CreatorName = collaboration.Creator.ApplicationUser.FirstName + " " + collaboration.Creator.ApplicationUser.LastName,
                Amount = collaboration.Amount,
                RequestDate = collaboration.CreatedAt,
                Status = collaboration.Status,
                Details = collaboration.Details,
                ReceiptUrl = collaboration.ReceiptUrl
            };

            _logger.LogInformation("Successfully fetched details for collaboration ID: {CollaborationId}", collaborationId);
            return new ServerResponse<CollaborationDto>(true)
            {
                ResponseCode = "00",
                ResponseMessage = "Collaboration details fetched successfully.",
                Data = collaborationDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching collaboration details.");
            return new ServerResponse<CollaborationDto>(false)
            {
                ResponseCode = "500",
                ResponseMessage = "An error occurred while fetching collaboration details."
            };
        }
    }

    public async Task<ServerResponse<FileStreamResult>> DownloadReceiptAsync(string collaborationId)
    {
        _logger.LogInformation("Downloading receipt for collaboration ID: {CollaborationId}", collaborationId);

        try
        {
            var collaboration = await _collaborationRepository.GetByIdAsync(collaborationId);
            if (collaboration == null || string.IsNullOrEmpty(collaboration.ReceiptUrl))
            {
                _logger.LogWarning("Receipt not found for collaboration ID: {CollaborationId}", collaborationId);
                return new ServerResponse<FileStreamResult>(false)
                {
                    ResponseCode = "404",
                    ResponseMessage = "Receipt not found."
                };
            }

            var receiptPath = Path.Combine(_webHostEnvironment.WebRootPath, collaboration.ReceiptUrl);

            var memory = new MemoryStream();
            using (var stream = new FileStream(receiptPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var fileResult = new FileStreamResult(memory, "application/pdf")
            {
                FileDownloadName = $"Receipt_{collaborationId}.pdf"
            };

            _logger.LogInformation("Successfully downloaded receipt for collaboration ID: {CollaborationId}", collaborationId);
            return new ServerResponse<FileStreamResult>(true)
            {
                ResponseCode = "00",
                ResponseMessage = "Receipt downloaded successfully.",
                Data = fileResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading receipt for collaboration ID: {CollaborationId}", collaborationId);
            return new ServerResponse<FileStreamResult>(false)
            {
                ResponseCode = "500",
                ResponseMessage = "An error occurred while downloading the receipt."
            };
        }
    }

    public async Task<ServerResponse<List<GetUserRequestDto>>> GetAllRequestsForCreatorAsync(string creatorId, string status)
    {
        _logger.LogInformation("Fetching all requests made to creator: {CreatorId} with status: {Status}", creatorId, status);

        // Fetch all requests where the creator is the logged-in user
        var query = _requestRepository.GetRequestsForCreator(creatorId);

        // Apply status filter (pending/completed)
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status == "Pending")
                query = query.Where(r => r.Status == RequestStatus.Pending);
            else if (status == "Completed")
                query = query.Where(r => r.Status == RequestStatus.Completed);
            else if (status == "All")
                query = query.Where(r => r.Status == RequestStatus.All);
        }

        var requests = await query.ToListAsync();

        // If no requests found, return a response indicating this
        if (requests == null || !requests.Any())
        {
            return new ServerResponse<List<GetUserRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No requests found for the creator.",
                Data = null
            };
        }

        // Convert requests to DTOs for the response
        var requestDtos = requests.Select(r => new GetUserRequestDto
        {
            RequestId = r.Id,
            UserFullName = $"{r.User.FirstName} {r.User.LastName}", // Full name of the Regular User who made the request
            Status = r.Status,
            CreatedAt = r.CreatedAt.UtcDateTime,
        }).ToList();

        return new ServerResponse<List<GetUserRequestDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Requests fetched successfully.",
            Data = requestDtos
        };
    }

    public async Task<ServerResponse<List<GetRequestDto>>> GetAllRequestsByUserAsync(string userId, string status)
    {
        _logger.LogInformation("Fetching all requests made by user: {UserId} with status: {Status}", userId, status);

        var query = _requestRepository.GetRequestsByUser(userId);

        // Apply status filter (pending/completed)
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status == "Pending")
                query = query.Where(r => r.Status == RequestStatus.Pending);
            else if (status == "Completed")
                query = query.Where(r => r.Status == RequestStatus.Completed);
            else if (status == "All")
                query = query.Where(r => r.Status == RequestStatus.All);
        }

        var requests = await query.ToListAsync();

        if (requests == null || !requests.Any())
        {
            return new ServerResponse<List<GetRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No requests found for the user.",
                Data = null
            };
        }

        // Convert requests to DTOs for the response
        var requestDtos = requests.Select(r => new GetRequestDto
        {
            RequestId = r.Id,
            CreatorFullName = $"{r.Creator.ApplicationUser.FirstName} {r.Creator.ApplicationUser.LastName}",
            Status = r.Status,
            CreatedAt = r.CreatedAt.UtcDateTime,
        }).ToList();

        return new ServerResponse<List<GetRequestDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Requests fetched successfully.",
            Data = requestDtos
        };
    }

    public async Task<ServerResponse<RequestDetailsDto>> GetRequestDetailsAsync(string requestId)
    {
        _logger.LogInformation("Fetching details for request: {RequestId}", requestId);

        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null)
        {
            return new ServerResponse<RequestDetailsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found."
            };
        }

        var requestDetails = new RequestDetailsDto
        {
            RequestId = request.Id,
            CreatorFullName = $"{request.Creator.ApplicationUser.FirstName} {request.Creator.ApplicationUser.LastName}",
            RequestType = request.RequestType,
            VideoUrl = request.VideoUrl,
            CreatedAt = request.CreatedAt.UtcDateTime,
            Script = request.Script,

        };

        return new ServerResponse<RequestDetailsDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Request details fetched successfully.",
            Data = requestDetails
        };
    }

    public async Task<ServerResponse<DisputeResponseDto>> OpenDisputeAsync(string requestId, OpenDisputeDto disputeDto)
    {
        _logger.LogInformation("Opening dispute for request: {RequestId}, Reason: {Reason}", requestId, disputeDto.DisputeReason);

        var request = await _requestRepository.GetRequestByIdAsync(requestId); // Use the requestId parameter directly
        if (request == null)
        {
            return new ServerResponse<DisputeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found."
            };
        }

        var dispute = new Dispute
        {
            Id = Guid.NewGuid().ToString(), // Assuming dispute ID is generated, not the same as requestId
            Reason = disputeDto.DisputeReason,
            DisputeMessage = disputeDto.Message,
            Status = DisputeStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RegularUserId = request.RegularUserId,
            CreatorId = request.CreatorId,
            Amount = request.Amount
        };

        var result = await _disputeRepository.CreateDispute(dispute);
        if (!result.IsSuccessful)
        {
            return new ServerResponse<DisputeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Error opening dispute."
            };
        }

        var regularUser = await _userRepository.GetUserByIdAsync(dispute.RegularUserId);
        var creator = await _userRepository.GetUserByIdAsync(dispute.CreatorId);

        var disputeResponse = new DisputeResponseDto
        {
            RequestId = requestId,  // Still returning the requestId in the response
            Reason = dispute.Reason,
            DisputeMessage = dispute.DisputeMessage,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt,
            RegularUserFullName = $"{regularUser.FirstName} {regularUser.LastName}",
            CreatorFullName = $"{creator.FirstName} {creator.LastName}",
            Amount = dispute.Amount
        };

        return new ServerResponse<DisputeResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Dispute opened successfully.",
            Data = disputeResponse
        };
    }

    public async Task<ServerResponse<VideoResponseDto>> SendVideoToUserAsync(string requestId, IFormFile video)
    {
        _logger.LogInformation("Sending video for request with ID: {RequestId}", requestId);

        // Check if the request exists
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
        {
            _logger.LogWarning("Request with ID: {RequestId} not found.", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Request not found."
                }
            };
        }

        // Check if the user ID is not null or empty
        if (string.IsNullOrEmpty(request.UserId))
        {
            _logger.LogWarning("User ID for request with ID: {RequestId} is null or empty.", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User ID is missing.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User ID is missing."
                }
            };
        }

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", request.UserId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
        }

        // Upload the video to Cloudinary
        var uploadResult = await _mediaService.UploadVideoAsync(video, "videos");

        if (uploadResult["Code"] != "200")
        {
            _logger.LogError("Failed to upload video for request with ID: {RequestId}", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Failed to upload video.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to upload video."
                }
            };
        }

        // Extract the publicId and folderName from the upload result
        var publicId = uploadResult["PublicId"]; // Assuming this is returned from your upload service
        var folderName = "videos"; // The folder where the video is uploaded

        // Get the download URL for the uploaded video
        var downloadUrlResponse = await _mediaService.GetDownloadUrlAsync(publicId, folderName);

        if (downloadUrlResponse["Code"] != "200")
        {
            _logger.LogError("Failed to generate download URL for request with ID: {RequestId}", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Failed to generate download URL.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to generate download URL."
                }
            };
        }

        var downloadUrl = downloadUrlResponse["DownloadUrl"];

        _logger.LogInformation("Video sent successfully for request with ID: {RequestId}", requestId);

        // Create the response DTO including the download URL
        var videoResponse = new VideoResponseDto
        {
            RequestId = request.Id,
            VideoUrl = downloadUrl,  // Use the generated download URL here
            UserId = user.Id,
            UserName = user.FullName
        };

        return new ServerResponse<VideoResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Video sent successfully.",
            Data = videoResponse
        };
    }

    public async Task<ServerResponse<List<FetchDisputeDto>>> FetchDisputesByCreatorAsync(string userId)
    {
        _logger.LogInformation("Attempting to fetch disputes for Creator with UserId: {UserId}", userId);

        // Fetch the user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found with UserId: {UserId}", userId);
            return new ServerResponse<List<FetchDisputeDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Not Found",
                    ResponseDescription = "User does not exist."
                }
            };
        }

        // Check if the logged-in user has the 'Creator' role
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(RolesConstant.Creator))
        {
            _logger.LogWarning("Unauthorized access attempt by UserId: {UserId}", userId);
            return new ServerResponse<List<FetchDisputeDto>>
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authorized to fetch disputes.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "401",
                    ResponseMessage = "Unauthorized",
                    ResponseDescription = "User is not a creator."
                }
            };
        }

        // Fetch disputes associated with the creator
        var disputes = await _disputeRepository.GetDisputesByCreator(user.Id)
            .Include(d => d.Request)
            .ThenInclude(r => r.User)
            .ToListAsync();

        if (disputes == null || !disputes.Any())
        {
            _logger.LogWarning("No disputes found for CreatorId: {CreatorId}", user.Id);
            return new ServerResponse<List<FetchDisputeDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No disputes found for the creator.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Not Found",
                    ResponseDescription = "No disputes available for the creator."
                }
            };
        }

        // Map to DisputeDto for the response
        var disputeDtos = disputes.Select(d => new FetchDisputeDto
        {
            DisputeId = d.Id,
            RegularUserFullName = $"{d.Request.User.FirstName} {d.Request.User.LastName}",
            CreatedAt = d.CreatedAt.UtcDateTime,
            DisputeType = DisputeType.DisputedVideo,
            Status = d.Status
        }).ToList();

        _logger.LogInformation("Successfully fetched {Count} disputes for CreatorId: {CreatorId}", disputeDtos.Count, user.Id);

        return new ServerResponse<List<FetchDisputeDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Disputes fetched successfully.",
            Data = disputeDtos
        };
    }


    public async Task<ServerResponse<DisputeDetailsDto>> GetDisputeDetailsByIdAsync(string disputeId, string userId)
    {
        _logger.LogInformation("Attempting to fetch dispute details for DisputeId: {DisputeId} by UserId: {UserId}", disputeId, userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with UserId: {UserId} not found.", userId);
            return new ServerResponse<DisputeDetailsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found."
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(RolesConstant.Creator.ToString()))
        {
            _logger.LogWarning("Unauthorized access attempt by UserId: {UserId}", userId);
            return new ServerResponse<DisputeDetailsDto>
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authorized to fetch dispute details.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "401",
                    ResponseMessage = "Unauthorized",
                    ResponseDescription = "User is not a creator."
                }
            };
        }

        // Fetch dispute details by disputeId and creatorId
        var dispute = await _disputeRepository.GetDisputeById(disputeId);
        if (dispute == null || dispute.Request.CreatorId != user.Id)
        {
            _logger.LogWarning("Dispute with DisputeId: {DisputeId} not found for CreatorId: {CreatorId}", disputeId, user.Id);
            return new ServerResponse<DisputeDetailsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Dispute not found or not associated with the creator.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Not Found",
                    ResponseDescription = "Dispute not found or does not belong to the creator."
                }
            };
        }

        // Map to DisputeDetailsDto
        var disputeDetailsDto = new DisputeDetailsDto
        {
            DisputeId = dispute.Id,
            RegularUserFullName = $"{dispute.Request.User.FirstName} {dispute.Request.User.LastName}",
            CreatedAt = dispute.CreatedAt.UtcDateTime,
            DisputeReason = dispute.Reason,
            DisputeMessage = dispute.DisputeMessage
        };

        _logger.LogInformation("Successfully fetched dispute details for DisputeId: {DisputeId}", disputeId);

        return new ServerResponse<DisputeDetailsDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Dispute details fetched successfully.",
            Data = disputeDetailsDto
        };
    }



    //// Method to download video with or without watermark
    //public async Task<ServerResponse<bool>> DownloadVideoAsync(string requestId, bool withWatermark)
    //{
    //    _logger.LogInformation("Processing video download for request: {RequestId}, WithWatermark: {WithWatermark}", requestId, withWatermark);

    //    var request = await _requestRepository.GetRequestByIdAsync(requestId);
    //    if (request == null)
    //    {
    //        return new ServerResponse<bool>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "Request not found."
    //        };
    //    }

    //    // If without watermark, charge the RegularUser
    //    if (!withWatermark)
    //    {
    //        var chargeResult = await _walletService.ChargeUser(request.UserId, request.VideoNoWatermarkPrice);
    //        if (!chargeResult.IsSuccessful)
    //        {
    //            return new ServerResponse<bool>
    //            {
    //                IsSuccessful = false,
    //                ResponseCode = "402",
    //                ResponseMessage = "Insufficient funds to download without watermark."
    //            };
    //        }
    //    }

    //    // Get video URL based on watermark preference
    //    var videoUrl = withWatermark ? request.VideoUrlWithWatermark : request.VideoUrlWithoutWatermark;
    //    var downloadResult = await _mediaService.DownloadVideo(videoUrl);

    //    if (!downloadResult.IsSuccessful)
    //    {
    //        return new ServerResponse<bool>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "500",
    //            ResponseMessage = "Error downloading video."
    //        };
    //    }

    //    return new ServerResponse<bool>
    //    {
    //        IsSuccessful = true,
    //        ResponseCode = "00",
    //        ResponseMessage = "Video downloaded successfully.",
    //        Data = true
    //    };
    //}

}

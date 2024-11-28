using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Lyvads.Application.Dtos.CreatorDtos;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using Lyvads.Domain.Constants;
using Microsoft.Extensions.Hosting;


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
    private readonly ILogger<AdminDashboardService> _logger;
    //private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IRegularUserRepository _regularUserRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediaService _mediaService;
    private readonly IWalletService _walletService;
    private readonly IUnitOfWork _unitOfWork;

    public CollaborationService(
        UserManager<ApplicationUser> userManager,
        ICollaborationRepository collaborationRepository,
        ICurrentUserService currentUserService,
        ICreatorRepository creatorRepository,
        IRepository repository,
        IUserRepository userRepository,
        IDisputeRepository disputeRepository,
        IRequestRepository requestRepository,
        ILogger<AdminDashboardService> logger,
        IMediaService mediaService,
        IWalletService walletService,
        IRegularUserRepository regularUserRepository,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
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
        _regularUserRepository = regularUserRepository;
        _httpContextAccessor = httpContextAccessor;
       //_webHostEnvironment = webHostEnvironment;
       _mediaService = mediaService;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
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
                RegularUserName = c.RegularUser?.ApplicationUser?.FirstName + " " + c.RegularUser?.ApplicationUser?.LastName,
                RegularUserPic = c.RegularUser?.ApplicationUser?.ImageUrl,
                CreatorName = c.Creator?.ApplicationUser?.FirstName + " " + c.Creator?.ApplicationUser?.LastName,
                CreatorPic = c.Creator?.ApplicationUser?.ImageUrl,
                TotalAmount = c.TotalAmount,
                RequestDate = c.CreatedAt,
                Status = c.Status.ToString(),
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

    public async Task<ServerResponse<CollaborationDetailsDto>> GetCollaborationDetailsAsync(string collaborationId)
    {
        _logger.LogInformation("Fetching details for collaboration ID: {CollaborationId}", collaborationId);

        try
        {
            var collaboration = await _collaborationRepository.GetByIdAsync(collaborationId);
            if (collaboration == null)
            {
                _logger.LogWarning("Collaboration not found with ID: {CollaborationId}", collaborationId);
                return new ServerResponse<CollaborationDetailsDto>(false)
                {
                    ResponseCode = "404",
                    ResponseMessage = "Collaboration not found."
                };
            }

            var collaborationDto = new CollaborationDetailsDto
            {
                Id = collaboration.Id,
                RegularUserName = collaboration.RegularUser?.ApplicationUser?.FirstName + " " + collaboration.RegularUser?.ApplicationUser?.LastName,
                RegularUserPic = collaboration.RegularUser?.ApplicationUser?.ImageUrl,
                CreatorName = collaboration.Creator?.ApplicationUser?.FirstName + " " + collaboration.Creator?.ApplicationUser?.LastName,
                CreatorPic = collaboration.Creator?.ApplicationUser?.ImageUrl,
                TotalAmount = collaboration.TotalAmount,
                RequestAmount = collaboration.RequestAmount,
                RequestDate = collaboration.CreatedAt,
                Status = collaboration.Status.ToString(),
                RequestType = collaboration.RequestType,
                FastTrackFee = collaboration.FastTrackFee,
                Script = collaboration.Script,
                VideoUrl = collaboration.VideoUrl,
            };

            _logger.LogInformation("Successfully fetched details for collaboration ID: {CollaborationId}", collaborationId);
            return new ServerResponse<CollaborationDetailsDto>(true)
            {
                ResponseCode = "00",
                ResponseMessage = "Collaboration details fetched successfully.",
                Data = collaborationDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching collaboration details.");
            return new ServerResponse<CollaborationDetailsDto>(false)
            {
                ResponseCode = "500",
                ResponseMessage = "An error occurred while fetching collaboration details."
            };
        }
    }

    public async Task<ServerResponse<bool>> FlagToggleCollaborationAsync(string collaborationId)
    {
        _logger.LogInformation("Flagging collaboration ID: {CollaborationId} as done.", collaborationId);

        try
        {
            // Retrieve the collaboration from the repository
            var collaboration = await _collaborationRepository.GetByIdAsync(collaborationId);

            if (collaboration == null)
            {
                _logger.LogWarning("Collaboration not found with ID: {CollaborationId}", collaborationId);
                return new ServerResponse<bool>(false)
                {
                    ResponseCode = "404",
                    ResponseMessage = "Collaboration not found."
                };
            }

            collaboration.Status = collaboration.Status == RequestStatus.Flagged ? RequestStatus.Pending : RequestStatus.Flagged;


            // Save changes to the repository
            await _collaborationRepository.UpdateAsync(collaboration);

            _logger.LogInformation("Successfully flagged collaboration ID: {CollaborationId} as done.", collaborationId);
            return new ServerResponse<bool>(true)
            {
                ResponseCode = "00",
                ResponseMessage = collaboration.Status == RequestStatus.Flagged
                ? "Collaboration flagged successfully."
                : "Collaboration pending successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging collaboration ID: {CollaborationId} as done.", collaborationId);
            return new ServerResponse<bool>(false)
            {
                ResponseCode = "500",
                ResponseMessage = "An error occurred while flagging the collaboration as done."
            };
        }
    }


    //public async Task<ServerResponse<FileStreamResult>> DownloadReceiptAsync(string collaborationId)
    //{
    //    _logger.LogInformation("Downloading receipt for collaboration ID: {CollaborationId}", collaborationId);

    //    try
    //    {
    //        var collaboration = await _collaborationRepository.GetByIdAsync(collaborationId);
    //        if (collaboration == null || string.IsNullOrEmpty(collaboration.ReceiptUrl))
    //        {
    //            _logger.LogWarning("Receipt not found for collaboration ID: {CollaborationId}", collaborationId);
    //            return new ServerResponse<FileStreamResult>(false)
    //            {
    //                ResponseCode = "404",
    //                ResponseMessage = "Receipt not found."
    //            };
    //        }

    //        var receiptPath = Path.Combine(_webHostEnvironment.WebRootPath, collaboration.ReceiptUrl);

    //        var memory = new MemoryStream();
    //        using (var stream = new FileStream(receiptPath, FileMode.Open))
    //        {
    //            await stream.CopyToAsync(memory);
    //        }
    //        memory.Position = 0;

    //        var fileResult = new FileStreamResult(memory, "application/pdf")
    //        {
    //            FileDownloadName = $"Receipt_{collaborationId}.pdf"
    //        };

    //        _logger.LogInformation("Successfully downloaded receipt for collaboration ID: {CollaborationId}", collaborationId);
    //        return new ServerResponse<FileStreamResult>(true)
    //        {
    //            ResponseCode = "00",
    //            ResponseMessage = "Receipt downloaded successfully.",
    //            Data = fileResult
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error downloading receipt for collaboration ID: {CollaborationId}", collaborationId);
    //        return new ServerResponse<FileStreamResult>(false)
    //        {
    //            ResponseCode = "500",
    //            ResponseMessage = "An error occurred while downloading the receipt."
    //        };
    //    }
    //}

    public async Task<ServerResponse<List<GetUserRequestDto>>> GetAllRequestsForCreatorAsync(string creatorId, RequestStatus status)
    {
        _logger.LogInformation("Fetching all requests made to creator: {CreatorId} with status: {Status}", creatorId, status);

        var creator = await _creatorRepository.GetCreatorByApplicationUserIdAsync(creatorId);

        if (creator == null || creator.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", creatorId);
            return new ServerResponse<List<GetUserRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }
        
        try
        {
            // Fetch all requests where the creator is the logged-in user
            var query = _requestRepository.GetRequestsForCreator(creator.Id);

            // Apply status filter (pending/completed/all)
            if (status == RequestStatus.Pending)
            {
                query = query.Where(r => r.Status == RequestStatus.Pending);
            }
            else if (status == RequestStatus.Completed)
            {
                query = query.Where(r => r.Status == RequestStatus.Completed);
            }
            else if (status == RequestStatus.All)
            {
                query = query.Where(r => r.Status == RequestStatus.All);
            }

            var requests = await query.ToListAsync();

            // If no requests found, return a response indicating this
            if (requests == null || !requests.Any())
            {
                return new ServerResponse<List<GetUserRequestDto>>
                {
                    IsSuccessful = true,
                    ResponseCode = "00",
                    ResponseMessage = "No requests found for the creator.",
                    Data = null!
                };
            }

            // Convert requests to DTOs for the response
            var requestDtos = requests.Select(r => new GetUserRequestDto
            {
                RequestId = r.Id,
                CreatorFullName = $"{r.Creator?.ApplicationUser?.FirstName} {r.Creator?.ApplicationUser?.LastName}", // Full name of the Creator
                UserFullName = $"{r.RegularUser!.ApplicationUser!.FirstName} {r.RegularUser!.ApplicationUser.LastName}", // Full name of the Regular User who made the request
                Status = r.Status.ToString(),
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching requests for user: {UserId}");
            return new ServerResponse<List<GetUserRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while fetching the requests.",
                Data = null!
            };
        }
    }

    public async Task<ServerResponse<List<GetRequestDto>>> GetAllRequestsByUserAsync(string userId, RequestStatus status)
    {
        _logger.LogInformation("Fetching all requests made by user: {UserId} with status: {Status}", userId, status);

        var regularUser = await _regularUserRepository.GetRegularUserByApplicationUserIdAsync(userId);
        if (regularUser == null || regularUser.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", userId);
            return new ServerResponse<List<GetRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }

        // Fetch requests for the user using RegularUserId
        var query = _requestRepository.GetRequestsByUser(regularUser.Id);

        if (status == RequestStatus.Pending)
        {
            query = query.Where(r => r.Status == RequestStatus.Pending);
        }
        else if (status == RequestStatus.Completed)
        {
            query = query.Where(r => r.Status == RequestStatus.Completed);
        }
        else if (status == RequestStatus.All)
        {
            // No need to apply filter, as you're fetching all statuses
        }

        var requests = await query.ToListAsync();

        if (requests == null || !requests.Any())
        {
            return new ServerResponse<List<GetRequestDto>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "No requests found for the user.",
                Data = new List<GetRequestDto>()
            };
        }

        // Convert requests to DTOs for the response, including RegularUser's ApplicationUser details
        var requestDtos = requests.Select(r => new GetRequestDto
        {
            RequestId = r.Id,
            CreatorFullName = $"{r.Creator?.ApplicationUser?.FirstName} {r.Creator?.ApplicationUser?.LastName}", // Full name of the Creator
            RegularUserFullName = $"{r.RegularUser?.ApplicationUser?.FirstName} {r.RegularUser?.ApplicationUser?.LastName}", // Full name of the RegularUser
            Status = r.Status.ToString(),
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
            CreatorFullName = $"{request.Creator?.ApplicationUser?.FirstName} {request.Creator?.ApplicationUser?.LastName}",
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

    public async Task<ServerResponse<DisputeResponseDto>> OpenDisputeAsync(string userId, string requestId, DisputeReasons disputeReason, OpenDisputeDto disputeDto)
    {
        _logger.LogInformation("Opening dispute for request: {RequestId}, Reason: {Reason}", requestId, disputeReason);

        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        var roles = await _userManager.GetRolesAsync(user!);
        if (roles == null || !roles.Contains("RegularUser"))
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to send videos.", userId);
            return new ServerResponse<DisputeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "You are not authorized to open disputes.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "You are not authorized to open disputes."
                }
            };
        }

        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null)
        {
            return new ServerResponse<DisputeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found."
            };
        }        

        var regularUser = await _regularUserRepository.GetByIdWithApplicationUser(request.RegularUserId!);
        if (regularUser == null || regularUser.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", request.RegularUserId);
            return new ServerResponse<DisputeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }
        //var regularUser = await _userRepository.GetUserByIdAsync(dispute.RegularUserId!);
        var creator = await _creatorRepository.GetCreatorByIdWithApplicationUser(request.CreatorId!);
        if (creator == null || creator.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", request.CreatorId);
            return new ServerResponse<DisputeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }

        var dispute = new Dispute
        {
            Id = Guid.NewGuid().ToString(),
            Reason = disputeReason,
            DisputeMessage = disputeDto.Message,
            Status = DisputeStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RegularUserId = request.RegularUserId,
            CreatorId = request.CreatorId,
            ApplicationUserId = userId,
            Amount = request.TotalAmount
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

        var disputeResponse = new DisputeResponseDto
        {
            RequestId = requestId,
            Reason = disputeReason.ToString(),
            DisputeMessage = dispute.DisputeMessage,
            Status = dispute.Status.ToString(),
            CreatedAt = dispute.CreatedAt,
            RegularUserFullName = $"{regularUser.ApplicationUser.FullName}",
            CreatorFullName = $"{creator.ApplicationUser.FullName}",
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
        if (string.IsNullOrEmpty(request.RegularUserId))
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
        var regularUser = await _regularUserRepository.GetByIdWithApplicationUser(request.RegularUserId);
        if (regularUser == null || regularUser.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", request.RegularUserId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }

        // Check if the creator is sending the video
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User not found"
                }
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Contains("Creator"))
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to send videos.", request.RegularUserId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "You are not authorized to send videos.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "You are not authorized to send videos."
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
                    ResponseMessage = "Failed to upload video.",
                    ResponseDescription = "Invalid type.",
                }
            };
        }

        var videoUrl = uploadResult["Url"];

        // Save the video URL to the database
        request.VideoUrl = videoUrl; 
        _repository.Update(request); 
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Video sent successfully for request with ID: {RequestId}", requestId);

        var videoResponse = new VideoResponseDto
        {
            RequestId = request.Id,
            VideoUrl = videoUrl,
            UserId = user.Id,
            CreatorName = user.FullName,
            SentToRegularUser = regularUser.ApplicationUser!.FullName,
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
        if (roles == null || !roles.Contains("Creator"))
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

        var creator = await _creatorRepository.GetCreatorByApplicationUserIdAsync(userId);
        if (creator == null || creator.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", userId);
            return new ServerResponse<List<FetchDisputeDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }

        // Fetch disputes associated with the creator
        var disputes = await _disputeRepository.GetDisputesByCreator(creator.Id)
            .Include(d => d.Request)
            .Include(r => r!.RegularUser)
            .ThenInclude(u => u.ApplicationUser)
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
            RegularUserFullName = $"{d.ApplicationUser!.FullName}",
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
        if (roles == null || !roles.Contains("Creator"))
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
        if (dispute == null)
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
            RegularUserFullName = $"{dispute.ApplicationUser.FullName}",
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

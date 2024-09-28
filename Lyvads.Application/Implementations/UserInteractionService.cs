using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Lyvads.Domain.Enums;
using Stripe.Checkout;
using Lyvads.Domain.Responses;
using Lyvads.Application.Dtos.AuthDtos;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Application.Implementations;

public class UserInteractionService : IUserInteractionService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository _repository;
    private readonly ILogger<UserInteractionService> _logger;
    private readonly ICreatorRepository _creatorRepository;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletService _walletService;
    private readonly IRequestRepository _requestRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public UserInteractionService(IUserRepository userRepository, 
        IConfiguration configuration,
        IRepository repository,
        ILogger<UserInteractionService> logger,
        ICreatorRepository creatorRepository, 
        IPaymentGatewayService paymentGatewayService,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IWalletService walletService,
        IRequestRepository requestRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _repository = repository;
        _logger = logger;
        _creatorRepository = creatorRepository;
        _paymentGatewayService = paymentGatewayService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _walletService = walletService;
        _requestRepository = requestRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServerResponse<string>> MakeRequestAsync(CreateRequestDto createRequestDto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "User.Error",
                        ResponseMessage = "User not found"
                    }
                };
            }

            var isRegularUser = await _userManager.IsInRoleAsync(user, "RegularUser");
            if (!isRegularUser)
            {
                _logger.LogWarning("User is not a regular user.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "RegularUser.Error",
                        ResponseMessage = "Regular User not found"
                    }
                };
            }

            var creator = await _creatorRepository.GetCreatorByIdAsync(createRequestDto.CreatorId);
            if (creator == null)
            {
                _logger.LogWarning("Creator not found.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Creator.Error",
                        ResponseMessage = "Creator not found"
                    }
                };
            }

            string? domain = _configuration.GetValue<string>("Lyvads_Client_URL");
            if (string.IsNullOrEmpty(domain))
            {
                _logger.LogError("Client URL is not configured.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Configuration.Error",
                        ResponseMessage = "Client URL is not configured."
                    }
                };
            }

            Session session;

            if (createRequestDto.PaymentMethod == PaymentMethod.ATMCard)
            {
                var paymentDto = new PaymentDTO
                {
                    Amount = (int)(createRequestDto.Amount * 100),
                    ProductName = "Request Payment",
                    ReturnUrl = "/return-url"
                };
                session = await _paymentGatewayService.CreateCardPaymentSessionAsync(paymentDto, domain);
            }
            else if (createRequestDto.PaymentMethod == PaymentMethod.Wallet)
            {
                var walletBalance = await _walletService.GetBalanceAsync(user.Id);
                if (walletBalance < createRequestDto.Amount)
                {
                    _logger.LogWarning("Insufficient wallet balance.");
                    return new ServerResponse<string>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "WalletBalance.Error",
                            ResponseMessage = "Insufficient wallet balance."
                        }
                    };
                }

                var result = await _walletService.DeductBalanceAsync(user.Id, createRequestDto.Amount);
                if (result)
                {
                    var request = new Request
                    {
                        Type = createRequestDto.Type,
                        Script = createRequestDto.Script,
                        CreatorId = createRequestDto.CreatorId,
                        UserId = user.Id,
                        RequestType = createRequestDto.RequestType,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        PaymentMethod = PaymentMethod.Wallet
                    };

                    var (requestResultIsSuccess, requestResultErrorMessage) = await _requestRepository.CreateRequestAsync(request);
                    if (requestResultIsSuccess)
                    {
                        _logger.LogInformation("Wallet payment successful and request created.");
                        return new ServerResponse<string>
                        {
                            IsSuccessful = true,
                            Data = "Wallet payment successful and request created"
                        };
                    }

                    _logger.LogWarning("Failed to create request after wallet payment.");
                    return new ServerResponse<string>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "CreateRequest.Error",
                            ResponseMessage = "Failed to create request after wallet payment."
                        }
                    };
                }

                _logger.LogWarning("Failed to process wallet payment.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "WalletPayment.Error",
                        ResponseMessage = "Failed to process wallet payment."
                    }
                };
            }
            else if (createRequestDto.PaymentMethod == PaymentMethod.Online)
            {
                var paymentDto = new PaymentDTO
                {
                    Amount = (int)(createRequestDto.Amount * 100),
                    ProductName = "Request Payment",
                    ReturnUrl = "/return-url"
                };
                session = await _paymentGatewayService.CreateOnlinePaymentSessionAsync(paymentDto, domain);
            }
            else
            {
                _logger.LogWarning("Unsupported payment method.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Payment.Error",
                        ResponseMessage = "Unsupported payment method."
                    }
                };
            }

            if (session == null)
            {
                _logger.LogWarning("Failed to create payment session.");
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Session.Error",
                        ResponseMessage = "Failed to create payment session."
                    }
                };
            }

            return new ServerResponse<string>
            {
                IsSuccessful = true,
                Data = session.Id
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while making request.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Exception.Error",
                    ResponseMessage = e.Message
                }
            };
        }
    }

    public async Task<ServerResponse<CommentResponseDto>> EditCommentAsync(string commentId, string userId, string newContent)
    {
        _logger.LogInformation("User with ID: {UserId} is attempting to edit comment with ID: {CommentId}", userId, commentId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User Not Found"
                }
            };
        }

        var comment = await _repository.GetAll<Comment>()
           .FirstOrDefaultAsync(x => x.Id == commentId && x.ApplicationUserId == user.Id);
        if (comment == null)
        {
            _logger.LogWarning("Comment with ID: {CommentId} not found.", commentId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Comment.Error",
                    ResponseMessage = "Comment not found."
                }
            };
        }

        if (comment.ApplicationUserId != userId)
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to edit comment with ID: {CommentId}", userId, commentId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Authorization.Error",
                    ResponseMessage = "You are not authorized to edit this comment."
                }
            };
        }

        comment.Content = newContent;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Update(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment with ID: {CommentId} was successfully updated by user with ID: {UserId}", commentId, userId);

        var commentResponse = new CommentResponseDto
        {
            CommentId = comment.Id,
            PostId = comment.PostId,
            UserId = comment.ApplicationUserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            CommentBy = comment.CommentBy
        };

        return new ServerResponse<CommentResponseDto>
        {
            IsSuccessful = true,
            Data = commentResponse
        };
    }

    public async Task<ServerResponse<object>> AddCommentAsync(string userId, string content)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Comment.Error",
                    ResponseMessage = "User not found"
                }
            };
        }

        var comment = new Comment
        {
            ApplicationUserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddCommentAsync(comment);
        _logger.LogInformation("Comment added successfully for user ID: {UserId}", userId);
        return new ServerResponse<object>
        {
            IsSuccessful = true
        };
    }

    public async Task<ServerResponse<object>> DeleteCommentAsync(string userId, string commentId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User Not Found"
                }
            };
        }

        var comment = await _repository.GetAll<Comment>()
           .FirstOrDefaultAsync(x => x.Id == commentId && x.ApplicationUserId == user.Id);
        if (comment == null)
        {
            _logger.LogWarning("Comment with ID: {CommentId} not found.", commentId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Comment.Error",
                    ResponseMessage = "Comment not found."
                }
            };
        }

        if (comment.ApplicationUserId != userId)
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to delete comment with ID: {CommentId}", userId, commentId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Authorization.Error",
                    ResponseMessage = "You are not authorized to delete this comment."
                }
            };
        }

        _repository.Remove(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment with ID: {CommentId} deleted successfully by user with ID: {UserId}", commentId, userId);
        return new ServerResponse<object>
        {
            IsSuccessful = true
        };
    }

    public async Task<ServerResponse<object>> LikeContentAsync(string userId, string contentId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Like.Error",
                    ResponseMessage = "User not found"
                }
            };
        }

        var like = new Like
        {
            UserId = userId,
            ContentId = contentId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddLikeAsync(like);
        _logger.LogInformation("User with ID: {UserId} liked content with ID: {ContentId}", userId, contentId);

        return new ServerResponse<object>
        {
            IsSuccessful = true
        };
    }

    public async Task<ServerResponse<object>> UnlikeContentAsync(string userId, string contentId)
    {
        var like = await _userRepository.GetLikeAsync(userId, contentId);
        if (like == null)
        {
            _logger.LogWarning("Like not found for UserId: {UserId} and ContentId: {ContentId}", userId, contentId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Unlike.Error",
                    ResponseMessage = "Like not found."
                }
            };
        }

        await _userRepository.RemoveLikeAsync(like);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User with ID: {UserId} unliked content with ID: {ContentId}", userId, contentId);

        return new ServerResponse<object>
        {
            IsSuccessful = true
        };
    }

    public async Task<ServerResponse<object>> FundWalletAsync(string userId, decimal amount)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Wallet.Error",
                    ResponseMessage = "User not found"
                }
            };
        }

        await _userRepository.UpdateWalletBalanceAsync(userId, amount);
        _logger.LogInformation("User with ID: {UserId} funded wallet by amount: {Amount}", userId, amount);

        return new ServerResponse<object>
        {
            IsSuccessful = true
        };
    }



    //public async Task<ServerResponse<object>> CreateRequestAsync(CreateRequestDto createRequestDto)
    //{
    //    _logger.LogInformation("Inside the CreateRequestAsync Method");

    //    var user = await _userRepository.GetUserByIdAsync(createRequestDto.UserId);
    //    var creator = await _creatorRepository.GetCreatorByIdAsync(createRequestDto.CreatorId);

    //    if (user == null)
    //    {
    //        _logger.LogWarning("User not found for ID: {UserId}", createRequestDto.UserId);
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "Request.Error",
    //                ResponseMessage = "User not found"
    //            }
    //        };
    //    }

    //    if (creator == null)
    //    {
    //        _logger.LogWarning("Creator not found for ID: {CreatorId}", createRequestDto.CreatorId);
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "Request.Error",
    //                ResponseMessage = "Creator not found"
    //            }
    //        };
    //    }

    //    var request = new Request
    //    {
    //        Type = createRequestDto.Type,
    //        Script = createRequestDto.Script,
    //        Amount = createRequestDto.Amount,
    //        CreatorId = createRequestDto.CreatorId,
    //        UserId = createRequestDto.UserId,
    //        RequestType = createRequestDto.RequestType,
    //        CreatedAt = DateTimeOffset.UtcNow,
    //        UpdatedAt = DateTimeOffset.UtcNow,
    //    };

    //    await _repository.Add(request);
    //    _logger.LogInformation("Request created successfully for User ID: {UserId} and Creator ID: {CreatorId}", user.Id, creator.Id);

    //    if (createRequestDto.PaymentMethod == PaymentMethod.Wallet)
    //    {
    //        var paymentResult = await FundWalletAsync(user.Id, createRequestDto.Amount);
    //        if (!paymentResult.IsSuccessful)
    //            return paymentResult;
    //    }
    //    else if (createRequestDto.PaymentMethod == PaymentMethod.Online)
    //    {
    //        var paymentResult = await _paymentGatewayService.ProcessPaymentAsync(createRequestDto.Amount, "usd", createRequestDto.Source ?? string.Empty, "Video request payment");
    //        if (!paymentResult.IsSuccessful)
    //            return paymentResult;
    //    }
    //    else if (createRequestDto.PaymentMethod == PaymentMethod.ATMCard)
    //    {
    //        // Handle ATM card payment if necessary
    //    }

    //    return new ServerResponse<object>
    //    {
    //        IsSuccessful = true
    //    };
    //}


}

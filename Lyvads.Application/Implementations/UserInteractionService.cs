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
using Lyvads.Shared.DTOs;
using Lyvads.Domain.Constants;
using Lyvads.Application.Utilities;
using Microsoft.AspNetCore.Mvc;
using Stripe;


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
    private readonly IPostRepository _postRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IRegularUserRepository _regularUserRepository;

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
        IPostRepository postRepository,
        IHttpContextAccessor httpContextAccessor,
        IRegularUserRepository regularUserRepository)
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
        _postRepository = postRepository;
        _regularUserRepository = regularUserRepository;
    }

    //public async Task<ServerResponse<string>> MakeRequestAsync(string creatorId, CreateRequestDto createRequestDto)
    //{
    //    try
    //    {
    //        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

    //        if (user == null)
    //        {
    //            _logger.LogWarning("User not found.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "User.Error",
    //                    ResponseMessage = "User not found"
    //                }
    //            };
    //        }

    //        var isRegularUser = await _userManager.IsInRoleAsync(user, "RegularUser");
    //        if (!isRegularUser)
    //        {
    //            _logger.LogWarning("User is not a regular user.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "RegularUser.Error",
    //                    ResponseMessage = "Regular User not found"
    //                }
    //            };
    //        }

    //        var creator = await _creatorRepository.GetCreatorByIdAsync(creatorId);
    //        if (creator == null)
    //        {
    //            _logger.LogWarning("Creator not found.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "Creator.Error",
    //                    ResponseMessage = "Creator not found"
    //                }
    //            };
    //        }

    //        string? domain = _configuration.GetValue<string>("Lyvads_Client_URL");
    //        if (string.IsNullOrEmpty(domain))
    //        {
    //            _logger.LogError("Client URL is not configured.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "Configuration.Error",
    //                    ResponseMessage = "Client URL is not configured."
    //                }
    //            };
    //        }

    //        Session session;

    //        if (createRequestDto.PaymentMethod == PaymentMethod.ATMCard)
    //        {
    //            var paymentDto = new PaymentDTO
    //            {
    //                Amount = (int)(createRequestDto.Amount * 100),
    //                ProductName = "Request Payment",
    //                ReturnUrl = "/return-url"
    //            };
    //            session = await _paymentGatewayService.CreateCardPaymentSessionAsync(paymentDto, domain);
    //        }
    //        else if (createRequestDto.PaymentMethod == PaymentMethod.Wallet)
    //        {
    //            var walletBalance = await _walletService.GetBalanceAsync(user.Id);
    //            if (walletBalance < createRequestDto.Amount)
    //            {
    //                _logger.LogWarning("Insufficient wallet balance.");
    //                return new ServerResponse<string>
    //                {
    //                    IsSuccessful = false,
    //                    ErrorResponse = new ErrorResponse
    //                    {
    //                        ResponseCode = "WalletBalance.Error",
    //                        ResponseMessage = "Insufficient wallet balance."
    //                    }
    //                };
    //            }

    //            var result = await _walletService.DeductBalanceAsync(user.Id, createRequestDto.Amount);
    //            if (result)
    //            {
    //                var request = new Request
    //                {
    //                    Script = createRequestDto.Script,
    //                    CreatorId = creatorId,
    //                    RegularUserId = user.Id,
    //                    RequestType = createRequestDto.RequestType,
    //                    CreatedAt = DateTimeOffset.UtcNow,
    //                    UpdatedAt = DateTimeOffset.UtcNow,
    //                    PaymentMethod = PaymentMethod.Wallet
    //                };

    //                var (requestResultIsSuccess, requestResultErrorMessage) = await _requestRepository.CreateRequestAsync(request);
    //                if (requestResultIsSuccess)
    //                {
    //                    _logger.LogInformation("Wallet payment successful and request created.");
    //                    return new ServerResponse<string>
    //                    {
    //                        IsSuccessful = true,
    //                        Data = "Wallet payment successful and request created"
    //                    };
    //                }

    //                _logger.LogWarning("Failed to create request after wallet payment.");
    //                return new ServerResponse<string>
    //                {
    //                    IsSuccessful = false,
    //                    ErrorResponse = new ErrorResponse
    //                    {
    //                        ResponseCode = "CreateRequest.Error",
    //                        ResponseMessage = "Failed to create request after wallet payment."
    //                    }
    //                };
    //            }

    //            _logger.LogWarning("Failed to process wallet payment.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "WalletPayment.Error",
    //                    ResponseMessage = "Failed to process wallet payment."
    //                }
    //            };
    //        }
    //        else if (createRequestDto.PaymentMethod == PaymentMethod.Online)
    //        {
    //            var paymentDto = new PaymentDTO
    //            {
    //                Amount = (int)(createRequestDto.Amount * 100),
    //                ProductName = "Request Payment",
    //                ReturnUrl = "/return-url"
    //            };
    //            session = await _paymentGatewayService.CreateOnlinePaymentSessionAsync(paymentDto, domain);
    //        }
    //        else
    //        {
    //            _logger.LogWarning("Unsupported payment method.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "Payment.Error",
    //                    ResponseMessage = "Unsupported payment method."
    //                }
    //            };
    //        }

    //        if (session == null)
    //        {
    //            _logger.LogWarning("Failed to create payment session.");
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "Session.Error",
    //                    ResponseMessage = "Failed to create payment session."
    //                }
    //            };
    //        }

    //        return new ServerResponse<string>
    //        {
    //            IsSuccessful = true,
    //            Data = session.Id
    //        };
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError(e, "Error occurred while making request.");
    //        return new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "Exception.Error",
    //                ResponseMessage = e.Message
    //            }
    //        };
    //    }
    //}

    public async Task<ServerResponse<MakeRequestDetailsDto>> MakeRequestAsync(string creatorId,
     AppPaymentMethod payment, RequestType requestType, CreateRequestDto createRequestDto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "User.Error",
                        ResponseMessage = "User not found"
                    }
                };
            }

            // Check if the user is a regular user
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User roles for {UserId}: {Roles}", user.Id, string.Join(", ", roles));

            if (!roles.Contains("RegularUser"))
            {
                _logger.LogWarning("User is not a regular user.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "RegularUser.Error",
                        ResponseMessage = "Regular User not found"
                    }
                };
            }

            var creator = await _creatorRepository.GetCreatorByIdAsync(creatorId);
            if (creator == null)
            {
                _logger.LogWarning("Creator not found.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Creator.Error",
                        ResponseMessage = "Creator not found"
                    }
                };
            }

            var regularUser = await _regularUserRepository.GetRegularUserByApplicationUserIdAsync(user.Id);
            if (regularUser == null)
            {
                _logger.LogWarning("Regular user not found.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "RegularUser.Error",
                        ResponseMessage = "Regular user not found."
                    }
                };
            }

            // Base amount from the request
            decimal baseAmount = createRequestDto.Amount;

            // Additional charges
            decimal fastTrackFee = createRequestDto.FastTrack ? 5000 : 0;
            decimal removeWatermarkFee = createRequestDto.RemoveWatermark ? 4000 : 0;
            decimal creatorPostFee = createRequestDto.CreatorPost ? 3000 : 0;

            // Calculate the total amount
            decimal totalAmount = baseAmount + fastTrackFee + removeWatermarkFee + creatorPostFee;

            // Generate the summary
            var paymentSummary = $"Subtotal: {baseAmount:C}\n" +
                                 $"Fast Track Fee: {fastTrackFee:C}\n" +
                                 $"Remove Watermark Fee: {removeWatermarkFee:C}\n" +
                                 $"Creator Post Fee: {creatorPostFee:C}\n" +
                                 $"Total Amount: {totalAmount:C}";

            _logger.LogInformation("Payment Summary: {PaymentSummary}", paymentSummary);

            string? domain = _configuration.GetValue<string>("Lyvads_Client_URL");
            if (string.IsNullOrEmpty(domain))
            {
                _logger.LogError("Client URL is not configured.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Configuration.Error",
                        ResponseMessage = "Client URL is not configured."
                    }
                };
            }

            // Process payment based on the selected payment method
            Session session;
            if (payment == AppPaymentMethod.ATMCard)
            {
                var paymentDto = new PaymentDTO
                {
                    Amount = (int)(totalAmount * 100),
                    ProductName = "Request Payment",
                    ReturnUrl = "/return-url"
                };
                session = await _paymentGatewayService.CreateCardPaymentSessionAsync(paymentDto, domain);
            }
            else if (payment == AppPaymentMethod.Wallet)
            {
                var walletBalance = await _walletService.GetBalanceAsync(user.Id);
                if (walletBalance < totalAmount)
                {
                    _logger.LogWarning("Insufficient wallet balance.");
                    return new ServerResponse<MakeRequestDetailsDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "WalletBalance.Error",
                            ResponseMessage = "Insufficient wallet balance."
                        }
                    };
                }

                var result = await _walletService.DeductBalanceAsync(user.Id, totalAmount);
                if (result)
                {
                    var request = new Request
                    {
                        Script = createRequestDto.Script!,
                        CreatorId = creatorId,
                        RegularUserId = regularUser.Id,
                        RequestType = requestType,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        PaymentMethod = AppPaymentMethod.Wallet,
                        Amount = totalAmount
                    };
                    // Ensure that FirstName and LastName are not null before concatenation
                    var creatorName = $"{creator.ApplicationUser?.FirstName} {creator.ApplicationUser?.LastName}";

                    var (requestResultIsSuccess, requestResultErrorMessage) = await _requestRepository.CreateRequestAsync(request);
                    if (requestResultIsSuccess)
                    {
                        _logger.LogInformation("Wallet payment successful and request created.");

                        // Return both payment summary and request details
                        var requestDetails = new MakeRequestDetailsDto
                        {
                            CreatorId = creatorId,
                            CreatorName = creatorName,
                            RequestType = requestType.ToString(),
                            Script = request.Script,
                            CreatedAt = request.CreatedAt,
                            PaymentMethod = request.ToString(),
                            Amount = totalAmount,
                            PaymentSummary = paymentSummary
                        };

                        return new ServerResponse<MakeRequestDetailsDto>
                        {
                            IsSuccessful = true,
                            Data = requestDetails
                        };
                    }

                    _logger.LogWarning("Failed to create request after wallet payment.");
                    return new ServerResponse<MakeRequestDetailsDto>
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
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "WalletPayment.Error",
                        ResponseMessage = "Failed to process wallet payment."
                    }
                };
            }
            else if (payment == AppPaymentMethod.Online)
            {
                var paymentDto = new PaymentDTO
                {
                    Amount = (int)(totalAmount * 100),
                    ProductName = "Request Payment",
                    ReturnUrl = "/return-url"
                };
                session = await _paymentGatewayService.CreateOnlinePaymentSessionAsync(paymentDto, domain);
            }
            else
            {
                _logger.LogWarning("Unsupported payment method.");
                return new ServerResponse<MakeRequestDetailsDto>
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
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "Session.Error",
                        ResponseMessage = "Failed to create payment session."
                    }
                };
            }

            // Return the payment session ID and request details
            var requestDetailsWithSession = new MakeRequestDetailsDto
            {
                CreatorId = creatorId,
                CreatorName = $"{creator.ApplicationUser!.FirstName} {creator.ApplicationUser.LastName}",
                RequestType = requestType.ToString(),
                Script = createRequestDto.Script,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentMethod = payment.ToString(),
                Amount = totalAmount,
                PaymentSummary = paymentSummary
            };

            return new ServerResponse<MakeRequestDetailsDto>
            {
                IsSuccessful = true,
                Data = requestDetailsWithSession
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while making request.");
            return new ServerResponse<MakeRequestDetailsDto>
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

    public async Task<ServerResponse<CommentResponseDto>> AddCommentOnPostAsync(string postId, string userId, string content)
    {
        _logger.LogInformation("User {UserId} is adding a comment on post {PostId}", userId, postId);

        // Check if user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found.", userId);
            return new ServerResponse<CommentResponseDto>
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

        // Create new comment
        var comment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            PostId = postId,
            CommentBy = $"{user.FirstName} {user.LastName}",
            ApplicationUserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Save the comment to the post
        await _repository.Add(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment added successfully to post {PostId} by user {UserId}", postId, userId);

        // Prepare the response DTO
        var commentResponse = new CommentResponseDto
        {
            CommentId = comment.Id,
            PostId = comment.PostId,
            UserId = comment.ApplicationUserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CommentBy = comment.CommentBy
        };

        return new ServerResponse<CommentResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Comment added to post successfully.",
            Data = commentResponse
        };
    }

    public async Task<ServerResponse<CommentResponseDto>> ReplyToCommentAsync(string parentCommentId, string userId, string content)
    {
        _logger.LogInformation("User {UserId} is replying to comment {ParentCommentId}", userId, parentCommentId);

        // Check if user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found.", userId);
            return new ServerResponse<CommentResponseDto>
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

        // Fetch the parent comment
        var parentComment = await _repository.GetCommentByIdAsync(parentCommentId);
        if (parentComment == null)
        {
            _logger.LogWarning("Parent comment {ParentCommentId} not found.", parentCommentId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Parent comment not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Parent comment not found."
                }
            };
        }

        // Create new reply comment
        var replyComment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            CommentBy = $"{user.FirstName} {user.LastName}",
            ApplicationUserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow,
            ParentCommentId = parentCommentId
        };

        // Add the reply to the parent comment's Replies collection
        if (parentComment.Replies == null)
        {
            parentComment.Replies = new List<Comment>();
        }
        parentComment.Replies.Add(replyComment);

        // Save the reply comment
        await _repository.Add(replyComment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Reply added successfully to comment {ParentCommentId} by user {UserId}", parentCommentId, userId);

        // Prepare the response DTO
        var replyResponse = new CommentResponseDto
        {
            CommentId = replyComment.Id,
            PostId = parentComment.PostId,
            UserId = replyComment.ApplicationUserId,
            Content = replyComment.Content,
            CreatedAt = replyComment.CreatedAt,
            CommentBy = replyComment.CommentBy,
            ParentCommentId = replyComment.ParentCommentId
        };

        return new ServerResponse<CommentResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Reply added successfully.",
            Data = replyResponse
        };
    }

    public async Task<ServerResponse<CommentResponseDto>> EditReplyAsync(string replyId, string userId, string newContent)
    {
        _logger.LogInformation("User {UserId} is editing reply {ReplyId}", userId, replyId);

        // Check if user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found.", userId);
            return new ServerResponse<CommentResponseDto>
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

        // Fetch the reply comment
        var replyComment = await _repository.GetCommentByIdAsync(replyId);
        if (replyComment == null)
        {
            _logger.LogWarning("Reply comment {ReplyId} not found.", replyId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Reply comment not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Reply comment not found."
                }
            };
        }

        // Check if the user is the owner of the reply comment
        if (replyComment.ApplicationUserId != userId)
        {
            _logger.LogWarning("User {UserId} is not authorized to edit reply {ReplyId}", userId, replyId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "You are not authorized to edit this reply.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "Unauthorized."
                }
            };
        }

        // Update the content of the reply comment
        replyComment.Content = newContent;
        replyComment.UpdatedAt = DateTimeOffset.UtcNow; // Optional: You may want to track the update time

        // Save the changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Reply {ReplyId} edited successfully by user {UserId}", replyId, userId);

        // Prepare the response DTO
        var replyResponse = new CommentResponseDto
        {
            CommentId = replyComment.Id,
            PostId = replyComment.PostId,
            UserId = replyComment.ApplicationUserId,
            Content = replyComment.Content,
            CreatedAt = replyComment.CreatedAt,
            UpdatedAt = replyComment.UpdatedAt, // Include updated date if needed
            CommentBy = replyComment.CommentBy,
            ParentCommentId = replyComment.ParentCommentId
        };

        return new ServerResponse<CommentResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Reply edited successfully.",
            Data = replyResponse
        };
    }

    public async Task<ServerResponse<object>> DeleteCommentAsync(string userId, string commentId)
    {
        // Check if the user exists
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

        // Fetch the comment
        var comment = await _repository.GetAll<Comment>()
           .FirstOrDefaultAsync(x => x.Id == commentId && x.ApplicationUserId == user.Id && !x.IsDeleted);  // Ensure not deleted
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

        // Check if the user is authorized to delete the comment
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

        // Soft delete by setting the IsDeleted flag to true
        comment.IsDeleted = true;
        _repository.Update(comment);  // Use update method for soft deletion

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment with ID: {CommentId} soft deleted successfully by user with ID: {UserId}", commentId, userId);
        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Comment deleted successfully.",
            Data = comment
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
                    ResponseCode = "06",
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
            IsSuccessful = true,
            ResponseCode = "00",
            Data = contentId
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
                    ResponseCode = "06",
                    ResponseMessage = "Like not found."
                }
            };
        }

        await _userRepository.RemoveLikeAsync(like);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User with ID: {UserId} unliked content with ID: {ContentId}", userId, contentId);

        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = contentId
        };
    }

    public async Task<ServerResponse<object>> FundWalletAsync(string userId, decimal amount, string paymentMethodId, string currency)
    {
        // Check if the user exists
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "06",
                    ResponseMessage = "User not found"
                }
            };
        }

        // Validate the currency if needed
        if (string.IsNullOrEmpty(currency))
        {
            _logger.LogWarning("Currency is not specified.");
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "Currency.Error",
                ResponseMessage = "Currency must be specified.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Currency.Error",
                    ResponseMessage = "Currency must be specified."
                }
            };
        }

        // Check if the user is a regular user
        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User roles for {UserId}: {Roles}", user.Id, string.Join(", ", roles));
        if (!roles.Contains("RegularUser"))
        {
            _logger.LogWarning("User is not a regular user.");
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "RegularUser.Error",
                    ResponseMessage = "Regular User not found",
                    ResponseDescription = "Only Regular Users can fund their wallets.",
                }
            };
        }

        // Fund the wallet via card and check if the payment is successful
        var fundingResult = await _walletService.FundWalletViaCardAsync(userId, amount, paymentMethodId, currency);
        if (!fundingResult.IsSuccessful)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Failed to fund wallet via card.",
                ErrorResponse = fundingResult.ErrorResponse
            };
        }

        // Update the wallet balance after successful payment
        await _userRepository.UpdateWalletBalanceAsync(userId, amount);
        _logger.LogInformation("User with ID: {UserId} funded wallet by amount: {Amount} in currency: {Currency}", userId, amount, currency);

        // Return success response including the amount funded, the card token, and user information
        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Wallet funded successfully",
            Data = new
            {
                UserId = userId,
                Amount = amount,
                Currency = currency,
                CardToken = paymentMethodId,
                UserName = user.UserName
            }
        };
    }

    public async Task<ServerResponse<string>> FundWalletViaOnlinePaymentAsync(string userId, decimal amount, string paymentMethodId, string currency)
    {
        // Validate the currency if needed
        if (string.IsNullOrEmpty(currency))
        {
            _logger.LogWarning("Currency is not specified.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "Currency.Error",
                ResponseMessage = "Currency must be specified.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Currency.Error",
                    ResponseMessage = "Currency must be specified."
                }
            };
        }

        // Create options for PaymentIntent
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),  // Convert amount to cents
            Currency = currency,
            PaymentMethod = paymentMethodId,  // PaymentMethodId from frontend
            ConfirmationMethod = "manual",   // Set to manual for frontend confirmation
            CaptureMethod = "automatic",
        };

        var service = new PaymentIntentService();
        try
        {
            // Create the payment intent via Stripe
            PaymentIntent intent = await service.CreateAsync(options);

            // Check if further action is required
            if (intent.Status == "requires_action" || intent.Status == "requires_confirmation")
            {
                return new ServerResponse<string>
                {
                    IsSuccessful = true,
                    ResponseCode = "200",
                    ResponseMessage = "Authentication required.",
                    Data = intent.ClientSecret  // Frontend will use this to complete payment authentication
                };
            }

            // Return success response if no further action is required
            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "200",
                ResponseMessage = "Payment intent created successfully.",
                Data = intent.ClientSecret  // Return the client secret for frontend
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Online payment failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to process online payment",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "StripeError",
                    ResponseDescription = stripeEx.Message
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Online payment failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while processing your payment.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "PaymentError",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    public async Task<ServerResponse<object>> ConfirmPaymentAsync(string paymentIntentId, string userId, decimal amount)
    {
        // Check if the user exists
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "06",
                    ResponseMessage = "User not found"
                }
            };
        }

        // Confirm the payment
        var confirmationResult = await _walletService.ConfirmPaymentAsync(paymentIntentId, userId, amount);
        if (!confirmationResult.IsSuccessful)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = confirmationResult.ResponseCode,
                ResponseMessage = confirmationResult.ResponseMessage,
                ErrorResponse = confirmationResult.ErrorResponse
            };
        }

        // If payment succeeded, update the wallet balance
        await _userRepository.UpdateWalletBalanceAsync(userId, amount);
        _logger.LogInformation("User with ID: {UserId} funded wallet by amount: {Amount}", userId, amount);

        // Return success response including payment details
        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Wallet funded successfully after payment confirmation.",
            Data = new
            {
                UserId = userId,
                Amount = amount,
                PaymentIntentId = paymentIntentId,
                UserName = user.UserName
            }
        };
    }

    public async Task<ServerResponse<int>> GetNumberOfLikesAsync(string postId)
    {
        var likesCount = await _userRepository.GetLikesCountAsync(postId);
        return new ServerResponse<int>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = likesCount
        };
    }

    public async Task<ServerResponse<int>> GetNumberOfCommentsAsync(string postId)
    {
        var commentsCount = await _repository.GetAll<Comment>()
            .CountAsync(c => c.PostId == postId);

        return new ServerResponse<int>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = commentsCount
        };
    }

    public async Task<ServerResponse<List<string>>> GetUsersWhoLikedPostAsync(string postId)
    {
        var userIds = await _userRepository.GetUserIdsWhoLikedPostAsync(postId);

        return new ServerResponse<List<string>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = userIds
        };
    }

    public async Task<ServerResponse<List<CommentResponseDto>>> GetAllCommentsOnPostAsync(string postId)
    {
        var comments = await _repository.GetAll<Comment>()
            .Where(c => c.PostId == postId)
            .ToListAsync();

        var commentResponses = comments.Select(c => new CommentResponseDto
        {
            CommentId = c.Id,
            UserId = c.ApplicationUserId,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CommentBy = c.CommentBy
        }).ToList();

        return new ServerResponse<List<CommentResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = commentResponses
        };
    }

    public async Task<ServerResponse<List<Comment>>> GetCommentsByPostIdAsync(string postId)
    {
        var post = await _postRepository.GetPostByIdAsync(postId);
        if (post == null)
        {
            _logger.LogWarning("Post not found for ID: {PostId}", postId);
            return new ServerResponse<List<Comment>>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Post.Error",
                    ResponseMessage = "Post not found"
                }
            };
        }

        var comments = await _postRepository.GetCommentsByPostIdAsync(postId);

        return new ServerResponse<List<Comment>>
        {
            IsSuccessful = true,
            Data = comments
        };
    }


    //public async Task<ServerResponse<object>> AddCreatorToFavoritesAsync(string userId, string creatorId)
    //{
    //    var user = await _userRepository.GetUserByIdAsync(userId);
    //    if (user == null)
    //    {
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "User.Error",
    //                ResponseMessage = "User not found."
    //            }
    //        };
    //    }

    //    await _userRepository.AddFavoriteAsync(userId, creatorId);

    //    return new ServerResponse<object>
    //    {
    //        IsSuccessful = true
    //    };
    //}

    public async Task<ServerResponse<object>> AddCreatorToFavoritesAsync(string userId, string creatorId)
    {
        // Fetch the user by ID
        var currentUser = await _userManager.FindByIdAsync(userId);
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User not found."
                }
            };
        }

        // Ensure the user is a RegularUser (you may need to adjust this check based on your user structure)
        if (await _userManager.IsInRoleAsync(currentUser!, RolesConstant.Creator))
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.RoleError",
                    ResponseMessage = "Only regular users can add creators to favorites."
                }
            };
        }

        // Ensure the creator exists
        var creator = await _creatorRepository.GetCreatorByIdAsync(creatorId);
        if (creator == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Creator.Error",
                    ResponseMessage = "Creator not found."
                }
            };
        }

        // Add creator to favorites
        await _userRepository.AddFavoriteAsync(userId, creatorId);

        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode =  "00",
            ResponseMessage = "Creator successfully added as favorite.",
            Data = creatorId
        };
    }
    
    public async Task<ServerResponse<object>> RemoveCreatorFromFavoritesAsync(string userId, string creatorId)
    {
        // Fetch the user by ID
        var currentUser = await _userManager.FindByIdAsync(userId);
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User not found."
                }
            };
        }

        // Ensure the user is a RegularUser (adjust this check based on your roles structure)
        if (await _userManager.IsInRoleAsync(currentUser!, RolesConstant.Creator))
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.RoleError",
                    ResponseMessage = "Only regular users can remove creators from favorites."
                }
            };
        }

        // Ensure the creator exists in the user's favorites
        var favorite = await _userRepository.GetFavoriteAsync(userId, creatorId);
        if (favorite == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Favorite.Error",
                    ResponseMessage = "Creator not found in your favorites."
                }
            };
        }

        // Remove the creator from favorites
        await _userRepository.RemoveFavoriteAsync(userId, creatorId);

        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Creator successfully removed from favorites.",
        };
    }

    public async Task<ServerResponse<object>> ToggleFavoriteAsync(string userId, string creatorId)
    {
        // Fetch the user by ID
        var currentUser = await _userManager.FindByIdAsync(userId);
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User not found."
                }
            };
        }

        // Ensure the user is a RegularUser (adjust this check based on your roles structure)
        if (await _userManager.IsInRoleAsync(currentUser!, RolesConstant.Creator))
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.RoleError",
                    ResponseMessage = "Only regular users can favorite or unfavorite creators."
                }
            };
        }

        // Ensure the creator exists
        var creator = await _creatorRepository.GetCreatorByIdAsync(creatorId);
        if (creator == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Creator.Error",
                    ResponseMessage = "Creator not found."
                }
            };
        }

        // Check if the creator is already in the user's favorites
        var favorite = await _userRepository.GetFavoriteAsync(userId, creatorId);
        if (favorite != null)
        {
            // If the creator is already a favorite, remove them from favorites
            await _userRepository.RemoveFavoriteAsync(userId, creatorId);

            return new ServerResponse<object>
            {
                IsSuccessful = true,
                ResponseCode = "06",
                ResponseMessage = "Creator successfully removed from favorites.",
            };
        }
        else
        {
            // If the creator is not a favorite, add them to favorites
            await _userRepository.AddFavoriteAsync(userId, creatorId);

            return new ServerResponse<object>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Creator successfully added as favorite.",
                Data = creatorId
            };
        }
    }

    public async Task<ServerResponse<List<CreatorResponseDto>>> GetFavoriteCreatorsAsync(string userId)
    {
        var favoriteCreators = await _userRepository.GetFavoriteCreatorsAsync(userId);

        var creatorResponses = favoriteCreators.Select(c => new CreatorResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            ProfilePicture = c.ProfilePicture,
            Industry = c.Industry,
            AppUserName = c.AppUserName,
        }).ToList();

        return new ServerResponse<List<CreatorResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = creatorResponses
        };
    }
  
    public async Task<ServerResponse<List<ViewPostResponseDto>>> GetAllPostsOfCreatorAsync(string creatorId)
    {
        var posts = await _repository.GetAll<Post>()
            .Where(p => p.CreatorId == creatorId)
            .ToListAsync();

        var postResponses = posts.Select(p => new ViewPostResponseDto
        {
            PostId = p.Id,
            Caption = p.Caption,
            CreatedAt = p.CreatedAt
        }).ToList();

        return new ServerResponse<List<ViewPostResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = postResponses
        };
    }

    public async Task<ServerResponse<PaginatorDto<IEnumerable<ViewPostResponseDto>>>> GetAllPostsOfCreatorAsync(string creatorId, PaginationFilter paginationFilter)
    {
        // Get the queryable list of posts for the creator
        var postsQuery = _repository.GetAll<Post>()
            .Where(p => p.CreatorId == creatorId);

        // Apply pagination
        var paginatedPosts = await postsQuery.PaginateAsync(paginationFilter);

        // Map the paginated posts to ViewPostResponseDto
        var postResponses = paginatedPosts.PageItems!.Select(p => new ViewPostResponseDto
        {
            PostId = p.Id,
            Caption = p.Caption,
            CreatedAt = p.CreatedAt
        }).ToList();

        // Return paginated data with the pagination info
        return new ServerResponse<PaginatorDto<IEnumerable<ViewPostResponseDto>>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = new PaginatorDto<IEnumerable<ViewPostResponseDto>>
            {
                CurrentPage = paginatedPosts.CurrentPage,
                PageSize = paginatedPosts.PageSize,
                NumberOfPages = paginatedPosts.NumberOfPages,
                PageItems = postResponses
            }
        };
    }

    public async Task<ServerResponse<PaginatorDto<IEnumerable<FeaturedCreatorDto>>>> GetFeaturedCreatorsAsync(PaginationFilter paginationFilter)
    {
        // Fetch creators from the repository as IQueryable, applying pagination
        var paginatedCreators = await _userRepository.GetCreatorsWithMostEngagementAndFollowersAsync()
                                                     .PaginateAsync(paginationFilter);

        // Map the paginated results to FeaturedCreatorDto
        var featuredCreators = paginatedCreators.PageItems!.Select(c => new FeaturedCreatorDto
        {
            Id = c.Id,
            Name = c.Name,
            ProfilePicture = c.ProfilePicture,
            Industry = c.Industry,
            EngagementCount = c.EngagementCount,
            FollowersCount = c.FollowersCount
        }).ToList();

        // Create a paginated response DTO
        var paginatedResult = new PaginatorDto<IEnumerable<FeaturedCreatorDto>>
        {
            CurrentPage = paginatedCreators.CurrentPage,
            PageSize = paginatedCreators.PageSize,
            NumberOfPages = paginatedCreators.NumberOfPages,
            PageItems = featuredCreators
        };

        return new ServerResponse<PaginatorDto<IEnumerable<FeaturedCreatorDto>>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = paginatedResult
        };
    }

    public async Task<ServerResponse<CreatorProfileDto>> ViewCreatorProfileAsync(string creatorId)
    {
        var creator = await _userRepository.GetCreatorByIdAsync(creatorId);
        if (creator == null)
        {
            return new ServerResponse<CreatorProfileDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "06",
                    ResponseMessage = "Creator not found."
                }
            };
        }

        // Calculate the sum of rates per request type
        var collabRates = creator?.CollabRates?
         .GroupBy(c => c.RequestType)
         .Select(group => new CollabRateDto
         {
             RequestType = group.Key!.ToString(), // RequestType from CollaborationRate
             TotalAmount = group.Sum(c => c.TotalAmount) // Summing Rate from CollaborationRate entity
         })
         .ToList();


        // Get all posts and convert to ViewPostDto
        var postResponse = await GetAllPostsOfCreatorAsync(creatorId);
        var posts = postResponse.Data.Select(p => new ViewPostDto
        {
            Id = p.PostId,
            Caption = p.Caption,
            CreatedAt = p.CreatedAt
        }).ToList();

        var creatorProfile = new CreatorProfileDto
        {
            Name = creator?.Name,
            AppUserName = creator?.AppUserName,
            FollowersCount = await _userRepository.GetFollowerCountAsync(creatorId),
            Posts = posts,
            Bio = creator?.Bio,
            Occupation = creator?.Occupation,
            CollabRates = collabRates // Correctly assigning the aggregated rates
        };

        return new ServerResponse<CreatorProfileDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = creatorProfile
        };
    }

    public async Task<ServerResponse<object>> FollowCreatorAsync(string userId, string creatorId)
    {
        // Check if the user is already following the creator
        var isFollowing = await _userRepository.IsUserFollowingCreatorAsync(userId, creatorId);

        if (isFollowing)
        {
            // Unfollow the creator
            await _userRepository.UnfollowCreatorAsync(userId, creatorId);

            return new ServerResponse<object>
            {
                ResponseCode = "00",
                IsSuccessful = true,
                ResponseMessage = "Successfully unfollowed the creator.",
                Data = creatorId
            };
        }
        else
        {
            // Follow the creator
            await _userRepository.FollowCreatorAsync(userId, creatorId);

            return new ServerResponse<object>
            {
                ResponseCode = "00",
                IsSuccessful = true,
                ResponseMessage = "Successfully followed the creator.",
                Data = creatorId
            };
        }
    }



    public async Task<ServerResponse<object>> UnfollowCreatorAsync(string userId, string creatorId)
    {
        await _userRepository.UnfollowCreatorAsync(userId, creatorId);

        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = creatorId
        };
    }

    public async Task<ServerResponse<LikeResponseDto>> ToggleLikeCommentAsync(string commentId, string userId)
    {
        _logger.LogInformation("User {UserId} is toggling like on comment {CommentId}", userId, commentId);

        var comment = await _repository.FindByCondition<Comment>(c => c.Id == commentId);
        if (comment == null)
        {
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Comment does not exist.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Comment does not exist."
                }
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User does not exist.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User does not exist."
                }
            };
        }

        // Check if the user has already liked the comment
        var existingLike = await _repository.FindByCondition<Like>(l => l.CommentId == commentId && l.UserId == userId);
        if (existingLike != null)
        {
            // If a like exists, remove it (unlike)
            _repository.Remove(existingLike);
            await _unitOfWork.SaveChangesAsync();

            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Comment unliked successfully."
            };
        }

        // If no like exists, add a new one
        var like = new Like
        {
            Id = Guid.NewGuid().ToString(),
            CommentId = commentId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            LikedBy = $"{user.FirstName} {user.LastName}"
        };

        await _repository.Add(like);
        await _unitOfWork.SaveChangesAsync();

        var likeResponse = new LikeResponseDto
        {
            LikeId = like.Id,
            CommentId = like.CommentId,
            UserId = like.UserId,
            CreatedAt = like.CreatedAt,
            LikedBy = like.LikedBy
        };

        return new ServerResponse<LikeResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Comment liked successfully.",
            Data = likeResponse
        };
    }


    public async Task<ServerResponse<LikeResponseDto>> ToggleLikePostAsync(string postId, string userId)
    {
        _logger.LogInformation("User {UserId} is toggling like on post {PostId}", userId, postId);

        var post = await _repository.FindByCondition<Post>(p => p.Id == postId);
        if (post == null)
        {
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Post does not exist.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Post does not exist."
                }
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User does not exist.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User does not exist."
                }
            };
        }

        // Check if the user has already liked the post
        var existingLike = await _repository.FindByCondition<Like>(l => l.PostId == postId && l.UserId == userId);
        if (existingLike != null)
        {
            // If a like exists, remove it (unlike)
            _repository.Remove(existingLike);
            await _unitOfWork.SaveChangesAsync();

            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Post unliked successfully."
            };
        }

        // If no like exists, add a new one
        var like = new Like
        {
            Id = Guid.NewGuid().ToString(),
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            LikedBy = $"{user.FirstName} {user.LastName}"
        };

        await _repository.Add(like);
        await _unitOfWork.SaveChangesAsync();

        var likeResponse = new LikeResponseDto
        {
            LikeId = like.Id,
            PostId = like.PostId,
            UserId = like.UserId,
            CreatedAt = like.CreatedAt,
            LikedBy = like.LikedBy
        };

        return new ServerResponse<LikeResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Post liked successfully.",
            Data = likeResponse
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

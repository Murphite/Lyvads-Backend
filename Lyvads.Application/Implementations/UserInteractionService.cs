﻿using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;
using Microsoft.EntityFrameworkCore;
using Lyvads.Shared.DTOs;
using Lyvads.Domain.Constants;
using Lyvads.Application.Utilities;
using System.Security.Claims;


namespace Lyvads.Application.Implementations;

public class UserInteractionService : IUserInteractionService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
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
    private readonly IChargeTransactionRepository _chargeTransactionRepository;
    private readonly IAdminActivityLogService _adminActivityLogService;
    private readonly ICurrentUserService _currentUserService;

    public UserInteractionService(
        IUserRepository userRepository,
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository, 
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
        IRegularUserRepository regularUserRepository, 
        IChargeTransactionRepository chargeTransactionRepository,
         IAdminActivityLogService adminActivityLogService,
        ICurrentUserService currentUserService
        )
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
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
        _chargeTransactionRepository = chargeTransactionRepository;
        _adminActivityLogService = adminActivityLogService;
        _currentUserService = currentUserService;
    }

    public async Task<ServerResponse<MakeRequestDetailsDto>> MakeRequestAsync(string creatorId,
AppPaymentMethod payment, CreateRequestDto createRequestDto)
    {
        using var transaction = await _repository.BeginTransactionAsync(); 

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

            if (!roles.Any(role => role.Equals("REGULARUSER", StringComparison.OrdinalIgnoreCase)))
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

            // Fetch creator and creator wallet
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

            var creatorWallet = await _walletRepository.GetWalletByCreatorIdAsync(creatorId);
            if (creatorWallet == null)
            {
                _logger.LogWarning("Creator wallet not found.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "CreatorWallet.Error",
                        ResponseMessage = "Creator wallet not found."
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

            // Fetch charges from the Charge table (or related service)
            var charges = await _chargeTransactionRepository.GetChargeDetailsAsync();
            if (charges == null)
            {
                _logger.LogWarning("Charge details not found.");
                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "ChargeDetails.Error",
                        ResponseMessage = "Failed to retrieve charge details."
                    }
                };
            }

            int baseAmount = createRequestDto.Amount;

            // Retrieve individual charges
            var withholdingTax = charges.FirstOrDefault(c => c.ChargeName == "Withholding Tax");
            var watermarkFee = charges.FirstOrDefault(c => c.ChargeName == "WaterMark");
            var creatorPostFee = charges.FirstOrDefault(c => c.ChargeName == "Creator Post Fee");
            var fastTrackFee = charges.FirstOrDefault(c => c.ChargeName == "Fast Track Fee");

            // Additional charges
            var feeDetails = CalculateTotalAmountWithCharges(createRequestDto.Amount, charges, createRequestDto);
            int totalAmount = feeDetails.TotalAmount;
            int FastTrackFee = feeDetails.FastTrackFee;
            int WatermarkFee = feeDetails.WatermarkFee;
            int CreatorPostFee = feeDetails.CreatorPostFee;
            int WithholdingTax = feeDetails.WithholdingTax;

            string paymentReference = string.Empty;
            string authorizationUrl = string.Empty;
            string cancelUrl = string.Empty;

            if (payment == AppPaymentMethod.Paystack)
            {
                var paystackResponse = await _paymentGatewayService.InitializePaymentAsync(totalAmount, user.Email!, user.FullName!);
                if (!paystackResponse.IsSuccessful || paystackResponse.Data == null)
                {
                    return new ServerResponse<MakeRequestDetailsDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "PaystackInitialization.Error",
                            ResponseMessage = "Failed to initialize payment with Paystack."
                        }
                    };
                }

                paymentReference = paystackResponse.Data.PaymentReference;
                authorizationUrl = paystackResponse.Data.AuthorizationUrl;
                cancelUrl = paystackResponse.Data.CancelUrl;

                // Create the request (before payment completion)
                var request = new Request
                {
                    Script = createRequestDto.Script!,
                    CreatorId = creatorId,
                    FastTrackFee = feeDetails.FastTrackFee,
                    RegularUserId = regularUser.Id,
                    RequestType = createRequestDto.RequestType,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    PaymentMethod = AppPaymentMethod.Paystack,
                    TotalAmount = totalAmount,
                    RequestAmount = baseAmount
                };

                var (requestResultIsSuccess, requestResultErrorMessage) = await _requestRepository.CreateRequestAsync(request);
                if (!requestResultIsSuccess)
                {
                    _logger.LogWarning("Failed to create request.");
                    return new ServerResponse<MakeRequestDetailsDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "RequestCreation.Error",
                            ResponseMessage = "Failed to create request."
                        }
                    };
                }

                // Create a transaction to track the payment
                var transact = new Transaction
                {
                    ApplicationUserId = user.Id,
                    Name = user.FullName,
                    Amount = totalAmount,
                    TrxRef = paymentReference,
                    Email = user.Email,
                    Status = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    RequestId = request.Id,
                    WalletId = null,
                    Type = TransactionType.Payment
                };

                // Save transaction to the database
                var transactResult = await _transactionRepository.CreateTransactionAsync(transact);
                if (transactResult == null)
                {
                    _logger.LogWarning("Transaction could not be created for request ID {RequestId}.", request.Id);
                    // Rollback the transaction and return an error
                    await transaction.RollbackAsync();
                    return new ServerResponse<MakeRequestDetailsDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "TransactionCreation.Error",
                            ResponseMessage = "Failed to create the transaction."
                        }
                    };
                }

                //await _walletService.CreditWalletAmountAsync(creator.Id, baseAmount + FastTrackFee);
                await SaveChargeTransactionsAsync(charges, totalAmount, request.Id, createRequestDto);
                // Send amounts to respective destinations
                
               // await _paymentGatewayService.CreditBusinessAccountAsync(WatermarkFee + CreatorPostFee + WithholdingTax);


                // Commit the transaction if all operations are successful
                await transaction.CommitAsync();

                // Return response including authorization URL for Paystack
                var requestDetailsWithSession = new MakeRequestDetailsDto
                {
                    RequestId = request.Id,
                    CreatorId = creatorId,
                    CreatorName = $"{creator.ApplicationUser!.FirstName} {creator.ApplicationUser.LastName}",
                    RequestType = createRequestDto.RequestType,
                    Script = createRequestDto.Script,
                    CreatedAt = DateTimeOffset.UtcNow,
                    PaymentMethod = payment.ToString(),
                    Status = transact.Status,
                    TotalAmount = totalAmount,
                    Subtotal = baseAmount,
                    WithholdingTax = feeDetails.WithholdingTax,
                    WatermarkFee = feeDetails.WatermarkFee,
                    CreatorPostFee = feeDetails.CreatorPostFee,
                    FastTrackFee = feeDetails.FastTrackFee,
                    //PaymentSummary = paymentSummary,
                    PaymentReference = paymentReference,
                    AuthorizationUrl = authorizationUrl,
                    CancelUrl = cancelUrl
                };

                return new ServerResponse<MakeRequestDetailsDto>
                {
                    IsSuccessful = true,
                    Data = requestDetailsWithSession
                };
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
                await _walletService.CreditWalletAmountAsync(creator.Id, baseAmount + FastTrackFee);
                //wait _paymentGatewayService.CreditBusinessAccountAsync(WatermarkFee + CreatorPostFee + WithholdingTax);
                if (result)
                {
                    var request = new Request
                    {
                        Script = createRequestDto.Script!,
                        CreatorId = creatorId,
                        FastTrackFee = feeDetails.FastTrackFee,
                        RegularUserId = regularUser.Id,
                        RequestType = createRequestDto.RequestType,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        PaymentMethod = AppPaymentMethod.Wallet,
                        TotalAmount = totalAmount,
                        RequestAmount = baseAmount,
                        WalletId = user.Wallet.Id
                    };

                    var (requestResultIsSuccess, requestResultErrorMessage) = await _requestRepository.CreateRequestAsync(request);
                    if (requestResultIsSuccess)
                    {
                        var transact = new Transaction
                        {
                            ApplicationUserId = user.Id,
                            Name = user.FullName,
                            Amount = totalAmount,
                            TrxRef = request.Id.ToString(),
                            Email = user.Email,
                            Status = true,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            RequestId = request.Id,
                            WalletId = user.Wallet.Id,
                            Type = TransactionType.Payment
                        };

                        await _transactionRepository.CreateTransactionAsync(transact);

                        await SaveChargeTransactionsAsync(charges, totalAmount, request.Id, createRequestDto);

                        // Commit the transaction
                        await transaction.CommitAsync();

                        var requestDetails = new MakeRequestDetailsDto
                        {
                            RequestId = request.Id,
                            CreatorId = creatorId,
                            CreatorName = $"{creator.ApplicationUser!.FirstName} {creator.ApplicationUser.LastName}",
                            RequestType = createRequestDto.RequestType,
                            Script = createRequestDto.Script,
                            CreatedAt = DateTimeOffset.UtcNow,
                            PaymentMethod = payment.ToString(),
                            Status = transact.Status,
                            TotalAmount = totalAmount,
                            Subtotal = baseAmount,
                            WithholdingTax = feeDetails.WithholdingTax,
                            WatermarkFee = feeDetails.WatermarkFee,
                            CreatorPostFee = feeDetails.CreatorPostFee,
                            FastTrackFee = feeDetails.FastTrackFee,
                            //PaymentSummary = paymentSummary

                        };

                        return new ServerResponse<MakeRequestDetailsDto>
                        {
                            IsSuccessful = true,
                            Data = requestDetails
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create request.");
                        return new ServerResponse<MakeRequestDetailsDto>
                        {
                            IsSuccessful = false,
                            ErrorResponse = new ErrorResponse
                            {
                                ResponseCode = "RequestCreation.Error",
                                ResponseMessage = "Failed to create request."
                            }
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("Wallet deduction failed.");
                    return new ServerResponse<MakeRequestDetailsDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "WalletDeduction.Error",
                            ResponseMessage = "Failed to deduct from wallet."
                        }
                    };
                }
            }

            return new ServerResponse<MakeRequestDetailsDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "InvalidPaymentMethod.Error",
                    ResponseMessage = "Invalid payment method selected."
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during request creation.");
            // Rollback the transaction if an exception occurs
            await transaction.RollbackAsync();

            return new ServerResponse<MakeRequestDetailsDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "RequestCreation.Error",
                    ResponseMessage = "An error occurred while processing the request."
                }
            };
        }
    }


    private FeeDetailsDto CalculateTotalAmountWithCharges(int amount, List<Charge> charges, CreateRequestDto requestDto)
    {
        int totalAmount = amount;
        int withholdingTaxAmount = 0;
        int watermarkFeeAmount = 0;
        int creatorPostFeeAmount = 0;
        int fastTrackFeeAmount = 0;

        var withholdingTax = charges.FirstOrDefault(c => c.ChargeName == "Withholding Tax");
        var watermarkFee = charges.FirstOrDefault(c => c.ChargeName == "WaterMark");
        var creatorPostFee = charges.FirstOrDefault(c => c.ChargeName == "Creator Post Fee");
        var fastTrackFee = charges.FirstOrDefault(c => c.ChargeName == "Fast Track Fee");

        // Calculate withholding tax if applicable
        if (withholdingTax != null)
        {
            withholdingTaxAmount = (int)(totalAmount * (withholdingTax.Percentage / 100m));
            totalAmount += withholdingTaxAmount;
        }

        // Calculate watermark fee if applicable and toggle is enabled
        if (requestDto.EnableWatermarkFee && watermarkFee != null && totalAmount >= watermarkFee.MinAmount)
        {
            watermarkFeeAmount = (int)(totalAmount * (watermarkFee.Percentage / 100m));
            totalAmount += watermarkFeeAmount;
        }

        // Calculate creator post fee if applicable and toggle is enabled
        if (requestDto.EnableCreatorFee && creatorPostFee != null && totalAmount >= creatorPostFee.MinAmount)
        {
            creatorPostFeeAmount = (int)(totalAmount * (creatorPostFee.Percentage / 100m));
            totalAmount += creatorPostFeeAmount;
        }

        // Calculate fast track fee if applicable and toggle is enabled
        if (requestDto.EnableFastTrackFee && fastTrackFee != null && totalAmount >= fastTrackFee.MinAmount)
        {
            fastTrackFeeAmount = (int)(totalAmount * (fastTrackFee.Percentage / 100m));
            totalAmount += fastTrackFeeAmount;
        }

        return new FeeDetailsDto
        {
            TotalAmount = totalAmount,
            WithholdingTax = withholdingTaxAmount,
            WatermarkFee = watermarkFeeAmount,
            CreatorPostFee = creatorPostFeeAmount,
            FastTrackFee = fastTrackFeeAmount
        };
    }


    private async Task SaveChargeTransactionsAsync(List<Charge> charges, int totalAmount, string requestId, CreateRequestDto requestDto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(requestId);
        if (transaction == null)
        {
            var wallet = await _walletRepository.GetByRequestIdAsync(requestId);
            if (wallet == null)
            {
                throw new InvalidOperationException($"Transaction with ID {requestId} not found in both Transaction and Wallet tables.");
            }

            transaction = new Transaction
            {
                Id = requestId,
                Wallet = wallet
            };
        }

        var chargeTransactions = new List<ChargeTransaction>();

        // Add applicable charges based on toggles
        if (requestDto.EnableWatermarkFee)
        {
            var watermarkFee = charges.FirstOrDefault(c => c.ChargeName == "WaterMark");
            if (watermarkFee != null && totalAmount >= watermarkFee.MinAmount)
            {
                chargeTransactions.Add(new ChargeTransaction
                {
                    ChargeName = watermarkFee.ChargeName,
                    Amount = (decimal)(totalAmount * (watermarkFee.Percentage / 100m)),
                    Description = "Watermark fee charge",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    TransactionId = requestId,
                    Transaction = transaction,
                    Status = transaction.Status ? CTransStatus.Paid : CTransStatus.NotPaid,
                    ApplicationUserId = transaction.ApplicationUserId!
                });
            }
        }

        if (requestDto.EnableCreatorFee)
        {
            var creatorPostFee = charges.FirstOrDefault(c => c.ChargeName == "Creator Post Fee");
            if (creatorPostFee != null && totalAmount >= creatorPostFee.MinAmount)
            {
                chargeTransactions.Add(new ChargeTransaction
                {
                    ChargeName = creatorPostFee.ChargeName,
                    Amount = (decimal)(totalAmount * (creatorPostFee.Percentage / 100m)),
                    Description = "Creator post fee charge",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    TransactionId = requestId,
                    Transaction = transaction,
                    Status = transaction.Status ? CTransStatus.Paid : CTransStatus.NotPaid,
                    ApplicationUserId = transaction.ApplicationUserId!
                });
            }
        }

        if (requestDto.EnableFastTrackFee)
        {
            var fastTrackFee = charges.FirstOrDefault(c => c.ChargeName == "Fast Track Fee");
            if (fastTrackFee != null && totalAmount >= fastTrackFee.MinAmount)
            {
                chargeTransactions.Add(new ChargeTransaction
                {
                    ChargeName = fastTrackFee.ChargeName,
                    Amount = (decimal)(totalAmount * (fastTrackFee.Percentage / 100m)),
                    Description = "Fast track fee charge",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    TransactionId = requestId,
                    Transaction = transaction,
                    Status = transaction.Status ? CTransStatus.Paid : CTransStatus.NotPaid,
                    ApplicationUserId = transaction.ApplicationUserId!
                });
            }
        }

        // Save all charge transactions
        if (chargeTransactions.Any())
        {
            await _chargeTransactionRepository.AddRangeAsync(chargeTransactions);
        }
    }

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
            CommentBy = comment.CommentBy,
            CommenterImage = user.ImageUrl,
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
            CommenterImage = user.ImageUrl,
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

    //public async Task<ServerResponse<object>> LikeContentAsync(string userId, string contentId)
    //{
    //    var user = await _userRepository.GetUserByIdAsync(userId);
    //    if (user == null)
    //    {
    //        _logger.LogWarning("User not found for ID: {UserId}", userId);
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "06",
    //                ResponseMessage = "User not found"
    //            }
    //        };
    //    }

    //    var like = new Like
    //    {
    //        UserId = userId,
    //        ContentId = contentId,
    //        CreatedAt = DateTimeOffset.UtcNow
    //    };

    //    await _userRepository.AddLikeAsync(like);
    //    _logger.LogInformation("User with ID: {UserId} liked content with ID: {ContentId}", userId, contentId);

    //    return new ServerResponse<object>
    //    {
    //        IsSuccessful = true,
    //        ResponseCode = "00",
    //        Data = contentId
    //    };
    //}

    //public async Task<ServerResponse<object>> UnlikeContentAsync(string userId, string contentId)
    //{
    //    var like = await _userRepository.GetLikeAsync(userId, contentId);
    //    if (like == null)
    //    {
    //        _logger.LogWarning("Like not found for UserId: {UserId} and ContentId: {ContentId}", userId, contentId);
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "06",
    //                ResponseMessage = "Like not found."
    //            }
    //        };
    //    }

    //    await _userRepository.RemoveLikeAsync(like);
    //    await _unitOfWork.SaveChangesAsync();

    //    _logger.LogInformation("User with ID: {UserId} unliked content with ID: {ContentId}", userId, contentId);

    //    return new ServerResponse<object>
    //    {
    //        IsSuccessful = true,
    //        ResponseCode = "00",
    //        Data = contentId
    //    };
    //}

    //public async Task<ServerResponse<object>> FundWalletAsync(string userId, decimal amount, string paymentMethodId, string? currency)
    //{
    //    // Check if the user exists
    //    var user = await _userRepository.GetUserByIdAsync(userId);
    //    if (user == null)
    //    {
    //        _logger.LogWarning("User not found for ID: {UserId}", userId);
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "06",
    //                ResponseMessage = "User not found"
    //            }
    //        };
    //    }

    //    // Validate the currency if needed
    //    if (string.IsNullOrEmpty(currency))
    //    {
    //        _logger.LogWarning("Currency is not specified.");
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "Currency.Error",
    //            ResponseMessage = "Currency must be specified.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "Currency.Error",
    //                ResponseMessage = "Currency must be specified."
    //            }
    //        };
    //    }

    //    // Check if the user is a regular user
    //    var roles = await _userManager.GetRolesAsync(user);
    //    _logger.LogInformation("User roles for {UserId}: {Roles}", user.Id, string.Join(", ", roles));
    //    if (!roles.Contains("RegularUser"))
    //    {
    //        _logger.LogWarning("User is not a regular user.");
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "RegularUser.Error",
    //                ResponseMessage = "Regular User not found",
    //                ResponseDescription = "Only Regular Users can fund their wallets.",
    //            }
    //        };
    //    }

    //    // Fund the wallet via card and check if the payment is successful
    //    var fundingResult = await _walletService.FundWalletViaCardAsync(userId, amount, paymentMethodId, currency);
    //    if (!fundingResult.IsSuccessful)
    //    {
    //        return new ServerResponse<object>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "500",
    //            ResponseMessage = "Failed to fund wallet via card.",
    //            ErrorResponse = fundingResult.ErrorResponse
    //        };
    //    }

    //    // Update the wallet balance after successful payment
    //    await _userRepository.UpdateWalletBalanceAsync(userId, amount);
    //    _logger.LogInformation("User with ID: {UserId} funded wallet by amount: {Amount} in currency: {Currency}", userId, amount, currency);

    //    // Return success response including the amount funded, the card token, and user information
    //    return new ServerResponse<object>
    //    {
    //        IsSuccessful = true,
    //        ResponseCode = "00",
    //        ResponseMessage = "Wallet funded successfully",
    //        Data = new
    //        {
    //            UserId = userId,
    //            Amount = amount,
    //            Currency = currency,
    //            CardToken = paymentMethodId,
    //            UserName = user.UserName
    //        }
    //    };
    //}

    //public async Task<ServerResponse<string>> FundWalletViaOnlinePaymentAsync(string userId, decimal amount, string paymentMethodId, string currency)
    //{
    //    // Validate the currency if needed
    //    if (string.IsNullOrEmpty(currency))
    //    {
    //        _logger.LogWarning("Currency is not specified.");
    //        return new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "Currency.Error",
    //            ResponseMessage = "Currency must be specified.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "Currency.Error",
    //                ResponseMessage = "Currency must be specified."
    //            }
    //        };
    //    }

    //    // Create options for PaymentIntent
    //    var options = new PaymentIntentCreateOptions
    //    {
    //        Amount = (long)(amount * 100),  // Convert amount to cents
    //        Currency = currency,
    //        PaymentMethod = paymentMethodId,  // PaymentMethodId from frontend
    //        ConfirmationMethod = "manual",   // Set to manual for frontend confirmation
    //        CaptureMethod = "automatic",
    //    };

    //    var service = new PaymentIntentService();
    //    try
    //    {
    //        // Create the payment intent via Stripe
    //        PaymentIntent intent = await service.CreateAsync(options);

    //        // Check if further action is required
    //        if (intent.Status == "requires_action" || intent.Status == "requires_confirmation")
    //        {
    //            return new ServerResponse<string>
    //            {
    //                IsSuccessful = true,
    //                ResponseCode = "200",
    //                ResponseMessage = "Authentication required.",
    //                Data = intent.ClientSecret  // Frontend will use this to complete payment authentication
    //            };
    //        }

    //        // Return success response if no further action is required
    //        return new ServerResponse<string>
    //        {
    //            IsSuccessful = true,
    //            ResponseCode = "200",
    //            ResponseMessage = "Payment intent created successfully.",
    //            Data = intent.ClientSecret  // Return the client secret for frontend
    //        };
    //    }
    //    catch (StripeException stripeEx)
    //    {
    //        _logger.LogError(stripeEx, "Online payment failed.");
    //        return new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "400",
    //            ResponseMessage = "Failed to process online payment",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "400",
    //                ResponseMessage = "StripeError",
    //                ResponseDescription = stripeEx.Message
    //            }
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Online payment failed.");
    //        return new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "500",
    //            ResponseMessage = "An error occurred while processing your payment.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "500",
    //                ResponseMessage = "PaymentError",
    //                ResponseDescription = ex.Message
    //            }
    //        };
    //    }
    //}

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
            //userid, user-image, appusername, counts of likes on posts, replies, comments, count of likes for post, 
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

        var collabRates = creator.CollabRates?
            .GroupBy(c => c.RequestType)
            .Select(group => new CollabRateDto
            {
                RateId = group.First().RateId,
                RequestType = group.Key?.ToString(),
                TotalAmount = group.Sum(c => c.TotalAmount)
            })
            .ToList();

        // Get the first 3 posts of the creator
        var postResponse = await GetAllPostsOfCreatorAsync(creatorId);
        var posts = postResponse.Data.Take(3).Select(p => new ViewPostDto
        {
            Id = p.PostId,
            Caption = p.Caption,
            CreatedAt = p.CreatedAt
        }).ToList();

        var creatorProfile = new CreatorProfileDto
        {
            Id = creator.Id,
            Name = creator.Name,
            ImageUrl = creator.ImageUrl,
            AppUserName = creator.AppUserName,
            FollowersCount = await _userRepository.GetFollowerCountAsync(creatorId),
            EngagementCount = creator.EngagementCount,
            Posts = posts,
            Bio = creator.Bio,
            Location = creator.Location,
            Occupation = creator.Occupation,
            CollabRates = collabRates
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

    public async Task<ServerResponse<bool>> CheckIfUserIsFollowingCreatorAsync(string userId, string creatorId)
    {
        // Check if the creator exists
        var creatorExists = await _userRepository.CreatorExistsAsync(creatorId);
        if (!creatorExists)
        {
            return new ServerResponse<bool>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "06",
                    ResponseMessage = "Creator not found."
                }
            };
        }

        // Check if the user is following the creator
        var isFollowing = await _userRepository.IsUserFollowingCreatorAsync(userId, creatorId);

        return new ServerResponse<bool>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = isFollowing
        };
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

    public async Task<ServerResponse<int>> GetCreatorsFollowingCountAsync(string userId)
    {
        var followingCount = await _userRepository.GetCreatorsFollowingCountAsync(userId);

        return new ServerResponse<int>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = followingCount
        };
    }


    public async Task<ServerResponse<int>> GetUsersFollowingCreatorCountAsync(string creatorId)
    {
        var followerCount = await _userRepository.GetUsersFollowingCreatorCountAsync(creatorId);

        return new ServerResponse<int>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = followerCount
        };
    }


    public async Task<ServerResponse<List<UserFollowerDto>>> GetUsersFollowingCreatorDetailsAsync(string creatorId)
    {
        var followers = await _userRepository.GetUsersFollowingCreatorDetailsAsync(creatorId);

        if (followers == null || !followers.Any())
        {
            return new ServerResponse<List<UserFollowerDto>>
            {
                IsSuccessful = false,
                ResponseCode = "06",
                ResponseMessage = "No followers found.",
                Data = new List<UserFollowerDto>()
            };
        }

        return new ServerResponse<List<UserFollowerDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = followers
        };
    }

    public async Task<ServerResponse<WalletBalanceDto>> ViewWalletBalanceAsync(string userId)
    {
        _logger.LogInformation("Viewing wallet balance for user with ID: {UserId}", userId);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID is null or empty.");
            return new ServerResponse<WalletBalanceDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "User ID is missing.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "User ID is missing."
                }
            };
        }

        var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

        if (wallet == null)
        {
            _logger.LogWarning("Wallet not found for user with ID: {UserId}.", userId);
            return new ServerResponse<WalletBalanceDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Wallet not found for the user.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Wallet not found."
                }
            };
        }

        var walletBalanceResponse = new WalletBalanceDto
        {
            CreatorId = userId,
            WalletBalance = wallet.Balance,
            UserName = wallet.ApplicationUser.FullName ?? "N/A"
        };

        _logger.LogInformation("Wallet balance retrieved for user with ID: {UserId}.", userId);

        return new ServerResponse<WalletBalanceDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Wallet balance successfully fetched.",
            Data = walletBalanceResponse
        };
    }


    public async Task<ServerResponse<bool>> CheckIfCreatorIsInUserFavoritesAsync(string userId, string creatorId)
    {
        // Check if the creator exists
        var creatorExists = await _userRepository.CreatorExistsAsync(creatorId);
        if (!creatorExists)
        {
            return new ServerResponse<bool>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "06",
                    ResponseMessage = "Creator not found."
                }
            };
        }

        // Check if the creator is in the user's favorites
        var isFavorite = await _userRepository.IsCreatorInUserFavoritesAsync(userId, creatorId);

        return new ServerResponse<bool>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            Data = isFavorite
        };
    }

    //public async Task<ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>> GetPostsForUserAsync(string userId, PaginationFilter paginationFilter)
    //{
    //    if (string.IsNullOrEmpty(userId))
    //    {
    //        _logger.LogWarning("User ID is null or empty.");
    //        return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "400",
    //            ResponseMessage = "User ID is null or empty."
    //        };
    //    }

    //    // Fetch user with their followers
    //    var user = await _userRepository.GetUserWithFollowersAsync(userId);
    //    if (user == null)
    //    {
    //        _logger.LogWarning($"User not found for ID: {userId}");
    //        return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "User not found."
    //        };
    //    }

    //    // Fetch the list of creator IDs the user is following
    //    var followingIds = await _userRepository.GetFollowingCreatorIdsAsync(userId);

    //    // Get filtered posts based on visibility and following status, applying pagination
    //    var paginatedPosts = await _postRepository.GetPaginatedPostsAsync(followingIds, paginationFilter);

    //    // Check if no posts were found
    //    if (!paginatedPosts.PageItems!.Any())
    //    {
    //        _logger.LogInformation("No posts found for the user.");
    //        return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
    //        {
    //            IsSuccessful = true, // Still return a successful response
    //            ResponseCode = "00",
    //            ResponseMessage = "No posts found for the user.",
    //            Data = new PaginatorDto<IEnumerable<GetPostDto>>
    //            {
    //                CurrentPage = paginatedPosts.CurrentPage,
    //                PageSize = paginatedPosts.PageSize,
    //                NumberOfPages = paginatedPosts.NumberOfPages,
    //                PageItems = new List<GetPostDto>() // Return an empty list
    //            }
    //        };
    //    }

    //    // Map posts to DTOs
    //    var postDtos = paginatedPosts.PageItems!.Select(p => new GetPostDto
    //    {
    //        PostId = p.Id,
    //        CreatorName = p.Creator.ApplicationUser?.FullName,
    //        CreatorImage = p.Creator.ApplicationUser?.ImageUrl,
    //        CreatorOccupation = p.Creator.ApplicationUser?.Occupation,
    //        CreatorAppUserName = p.Creator.ApplicationUser?.AppUserName,
    //        Caption = p.Caption,
    //        Location = p.Location,
    //        Visibility = p.Visibility.ToString(),
    //        CreatedAt = p.CreatedAt,
    //        MediaUrls = p.MediaFiles.Select(m => m.Url).ToList()
    //    }).ToList();

    //    _logger.LogInformation("Retrieved {PostCount} posts for the user.", postDtos.Count);

    //    // Return paginated response
    //    return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
    //    {
    //        IsSuccessful = true,
    //        ResponseCode = "00",
    //        ResponseMessage = "Posts retrieved successfully.",
    //        Data = new PaginatorDto<IEnumerable<GetPostDto>>
    //        {
    //            CurrentPage = paginatedPosts.CurrentPage,
    //            PageSize = paginatedPosts.PageSize,
    //            NumberOfPages = paginatedPosts.NumberOfPages,
    //            PageItems = postDtos
    //        }
    //    };
    //}
    public async Task<ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>> GetPostsForUserAsync(string userId, PaginationFilter paginationFilter)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID is null or empty.");
            return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "User ID is null or empty."
            };
        }

        var user = await _userRepository.GetUserWithFollowersAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User not found for ID: {userId}");
            return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found."
            };
        }

        var followingIds = await _userRepository.GetFollowingCreatorIdsAsync(userId);
        var paginatedPosts = await _postRepository.GetPaginatedPostsAsync(followingIds, paginationFilter);

        if (!paginatedPosts.PageItems!.Any())
        {
            _logger.LogInformation("No posts found for the user.");
            return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "No posts found for the user.",
                Data = new PaginatorDto<IEnumerable<GetPostDto>>
                {
                    CurrentPage = paginatedPosts.CurrentPage,
                    PageSize = paginatedPosts.PageSize,
                    NumberOfPages = paginatedPosts.NumberOfPages,
                    PageItems = new List<GetPostDto>()
                }
            };
        }

        var postIds = paginatedPosts.PageItems.Select(p => p.Id).ToList();
        var userLikes = await _postRepository.GetLikesByUserAndPostsAsync(userId, postIds);
        var userComments = await _postRepository.GetCommentsByUserAndPostsAsync(userId, postIds);
        var userReplies = await _postRepository.GetRepliesByUserAndPostsAsync(userId, postIds);

        var postDtos = paginatedPosts.PageItems!.Select(p => new GetPostDto
        {
            PostId = p.Id,
            CreatorName = p.Creator.ApplicationUser?.FullName,
            CreatorImage = p.Creator.ApplicationUser?.ImageUrl,
            CreatorId = p.Creator.Id,
            CreatorOccupation = p.Creator.ApplicationUser?.Occupation,
            CreatorAppUserName = p.Creator.ApplicationUser?.AppUserName,
            Caption = p.Caption,
            Location = p.Location,
            Visibility = p.Visibility.ToString(),
            CreatedAt = p.CreatedAt,
            MediaUrls = p.MediaFiles.Select(m => m.Url).ToList(),
            IsLikedByUser = userLikes.Any(l => l.PostId == p.Id),
            //HasCommentedByUser = userComments.Any(c => c.PostId == p.Id),
            //HasRepliedByUser = userReplies.Any(r => r.PostId == p.Id), // ✅ FIXED
            //LikedByUserIds = userLikes.Where(l => l.PostId == p.Id).Select(l => l.UserId).ToList(),
           // CommentedByUserIds = userComments.Where(c => c.PostId == p.Id).Select(c => c.ApplicationUserId).ToList(),
            //RepliedByUserIds = userReplies.Where(r => r.PostId == p.Id)
                //.Select(r => r.RegularUserId ?? r.ApplicationUserId) // ✅ FIXED
                //.Where(id => id != null)
                //.ToList()!
        }).ToList();

        return new ServerResponse<PaginatorDto<IEnumerable<GetPostDto>>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Posts retrieved successfully.",
            Data = new PaginatorDto<IEnumerable<GetPostDto>>
            {
                CurrentPage = paginatedPosts.CurrentPage,
                PageSize = paginatedPosts.PageSize,
                NumberOfPages = paginatedPosts.NumberOfPages,
                PageItems = postDtos
            }
        };
    }

    //id of user who created post

    public async Task<ServerResponse<GetPostWithCommentsDto>> GetPostDetailsWithCommentsAsync(string postId)
    {
        if (string.IsNullOrEmpty(postId))
        {
            _logger.LogWarning("Post ID is null or empty.");
            return new ServerResponse<GetPostWithCommentsDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Post ID is null or empty."
            };
        }

        // Fetch the post with comments and replies
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        if (post == null)
        {
            _logger.LogWarning($"Post not found for ID: {postId}");
            return new ServerResponse<GetPostWithCommentsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Post not found."
            };
        }

        // Map the post details to a DTO
        var postDto = new GetPostWithCommentsDto
        {
            PostId = post.Id,
            CreatorName = post.Creator.ApplicationUser?.FullName,
            Caption = post.Caption,
            Location = post.Location,
            Visibility = post.Visibility.ToString(),
            CreatedAt = post.CreatedAt,
            MediaUrls = post.MediaFiles.Select(m => m.Url).ToList(),
            Comments = post.Comments.Select(c => new GetCommentDto
            {
                CommentId = c.Id,
                Content = c.Content,
                CommentedBy = c.CommentBy,
                CreatedAt = c.CreatedAt,
                Replies = c.Replies.Select(r => new GetReplyDto
                {
                    ReplyId = r.Id,
                    Content = r.Content,
                    RepliedBy = r.ParentComment.CommentBy,  
                    CreatedAt = r.CreatedAt
                }).ToList()
            }).ToList()
        };

        // Return the response
        return new ServerResponse<GetPostWithCommentsDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Post details retrieved successfully.",
            Data = postDto
        };
    }


}

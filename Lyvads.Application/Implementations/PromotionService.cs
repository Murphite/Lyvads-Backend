

using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementations;

public class PromotionService : IPromotionPlanService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository _repository;
    private readonly IMediaService _mediaService;
    private readonly ICreatorRepository _creatorRepository;
    private readonly IPromotionSubRepository _promotionSubRepository;
    private readonly IPromotionPlanRepository _promotionPlanRepository;
    private readonly IImpressionRepository _impressionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminDashboardService> _logger;
    private readonly IPaymentGatewayService _paymentGatewayService;


    public PromotionService(
        UserManager<ApplicationUser> userManager,
        IRepository repository,
        IMediaService mediaService,
        ICreatorRepository creatorRepository,
        IPromotionSubRepository promotionSubRepository,
        IPromotionPlanRepository promotionPlanRepository,
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IPaymentGatewayService paymentGatewayService,
        ILogger<AdminDashboardService> logger,
        IImpressionRepository impressionRepository)
    {
        _userManager = userManager;
        _paymentGatewayService = paymentGatewayService;
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
        _mediaService = mediaService;
        _creatorRepository = creatorRepository;
        _promotionSubRepository = promotionSubRepository;
        _promotionPlanRepository = promotionPlanRepository;
        _transactionRepository = transactionRepository;
        _impressionRepository = impressionRepository;
    }


    public async Task<ServerResponse<PromotionPlan>> CreatePromotionPlanAsync(CreatePromotionPlanDto planDto)
    {
        if (string.IsNullOrWhiteSpace(planDto.Name) || planDto.Price <= 0 || planDto.DurationInDays <= 0)
        {
            return new ServerResponse<PromotionPlan>
            {
                IsSuccessful = false,
                ResponseMessage = "Invalid promotion plan details provided."
            };
        }

        var promotionPlan = new PromotionPlan
        {
            Name = planDto.Name,
            Description = planDto.Description,
            Price = planDto.Price,
            DurationInDays = planDto.DurationInDays,
            CreatedAt = DateTime.UtcNow, // Set CreatedAt
            UpdatedAt = DateTime.UtcNow  // Set UpdatedAt
        };

        await _promotionPlanRepository.AddAsync(promotionPlan);

        return new ServerResponse<PromotionPlan>
        {
            IsSuccessful = true,
            Data = promotionPlan,
            ResponseMessage = "Promotion plan created successfully."
        };
    }

    public async Task<ServerResponse<PaginatorDto<IEnumerable<PromotionPlan>>>> GetAvailablePromotionPlansAsync(PaginationFilter paginationFilter)
    {
        var plans = await _promotionPlanRepository.GetPaginatedPlansAsync(paginationFilter);

        if (plans.PageItems == null || !plans.PageItems.Any())
        {
            return new ServerResponse<PaginatorDto<IEnumerable<PromotionPlan>>>
            {
                IsSuccessful = false,
                ResponseMessage = "No promotion plans available."
            };
        }

        return new ServerResponse<PaginatorDto<IEnumerable<PromotionPlan>>>
        {
            IsSuccessful = true,
            Data = new PaginatorDto<IEnumerable<PromotionPlan>>
            {
                CurrentPage = plans.CurrentPage,
                PageSize = plans.PageSize,
                NumberOfPages = plans.NumberOfPages,
                PageItems = plans.PageItems
            },
            ResponseMessage = "List of available promotion plans retrieved successfully."
        };
    }

    public async Task<ServerResponse<SubscriptionPaymentResponseDto>> SubscribeToPromotionPlanAsync(string planId, string creatorId)
    {
        // Begin a new database transaction
        using var transaction = await _repository.BeginTransactionAsync();

        // Fetch the promotion plan by ID
        var plan = await _promotionPlanRepository.GetByIdAsync(planId);
        if (plan == null)
        {
            return new ServerResponse<SubscriptionPaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Promotion plan not found."
            };
        }

        // Fetch the creator by ID
        var creator = await _creatorRepository.GetCreatorByIdAsync(creatorId);
        if (creator == null || creator.ApplicationUser == null)
        {
            return new ServerResponse<SubscriptionPaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Creator not found."
            };
        }

        string paymentReference = string.Empty;
        string authorizationUrl = string.Empty;
        string cancelUrl = string.Empty;

        // Initialize payment through the payment gateway service
        var response = await _paymentGatewayService.InitializePaymentAsync((int)plan.Price, creator.ApplicationUser.Email, creator.ApplicationUser.FullName);
        if (!response.IsSuccessful)
        {
            return new ServerResponse<SubscriptionPaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = response.ResponseMessage
            };
        }

        paymentReference = response.Data.PaymentReference;
        authorizationUrl = response.Data.AuthorizationUrl;
        cancelUrl = response.Data.CancelUrl;

        // Create a subscription entry (inactive until payment is confirmed)
        var subscription = new PromotionSubscription
        {
            CreatorId = creator.Id,
            PromotionPlanId = planId,
            PaymentReference = paymentReference,
            SubscriptionDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(plan.DurationInDays),
            IsActive = false,  // Set initially to false, will be updated later
            ApplicationUserId = creator.ApplicationUser.Id
        };

        // Save the subscription entry
        await _promotionSubRepository.AddAsync(subscription);

        // Create a transaction entry for the payment
        var transact = new Transaction
        {
            ApplicationUserId = creator.ApplicationUser.Id,
            Name = creator.ApplicationUser.FullName,
            Amount = plan.Price,
            TrxRef = paymentReference,
            Email = creator.ApplicationUser.Email,
            Status = false, // Status set to false initially, will be updated upon successful payment
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            RequestId = null,
            WalletId = null,
            Type = TransactionType.PromotionSubscription
        };

        // Save the transaction
        var transactResult = await _transactionRepository.CreateTransactionAsync(transact);
        if (transactResult == null)
        {
            _logger.LogWarning("Transaction could not be created for plan ID {PlanId}.", plan.Id);

            // Rollback the transaction in case of failure
            await transaction.RollbackAsync();

            return new ServerResponse<SubscriptionPaymentResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "TransactionCreation.Error",
                    ResponseMessage = "Failed to create the transaction."
                }
            };
        }        

        // Prepare the payment response DTO
        var paymentResponseDto = new SubscriptionPaymentResponseDto
        {
            SubscriptionId = subscription.Id,
            PaymentReference = response.Data.PaymentReference,
            AuthorizationUrl = response.Data.AuthorizationUrl,
            UserName = creator.ApplicationUser.FullName,
            UserEmail = creator.ApplicationUser.Email,
            Amount = plan.Price,
            DateCreated = DateTime.UtcNow
        };

        // Commit the transaction if everything is successful
        await transaction.CommitAsync();

        return new ServerResponse<SubscriptionPaymentResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Subscription initiated. Please complete payment.",
            Data = paymentResponseDto
        };
    }

    public async Task<ServerResponse<PaginatorDto<IEnumerable<SubscribedCreatorDto>>>> GetSubscribedCreatorsAsync(PaginationFilter paginationFilter)
    {
        var subscriptions = await _promotionSubRepository.GetPaginatedSubscriptionsAsync(paginationFilter);

        var subscribedCreators = new List<SubscribedCreatorDto>();

        foreach (var subscription in subscriptions.PageItems.Where(s => s.IsActive))
        {
            var creator = await _creatorRepository.GetCreatorByIdAsync(subscription.ApplicationUserId);
            var plan = await _promotionPlanRepository.GetByIdAsync(subscription.PromotionPlanId);

            subscribedCreators.Add(new SubscribedCreatorDto
            {
                SubscriptionId = subscription.Id,
                CreatorId = subscription.CreatorId,
                CreatorName = creator?.ApplicationUser.FullName!,
                CreatorImage = creator?.ApplicationUser.ImageUrl!,
                CreatorOccupation = creator?.ApplicationUser.Occupation!,
                Email = creator?.ApplicationUser?.Email!,
                AmountPaid = plan.Price,
                PlanName = plan?.Name!,
                SubscriptionDate = subscription.SubscriptionDate,
                ExpiryDate = subscription.ExpiryDate
            });
        }

        if (!subscribedCreators.Any())
        {
            return new ServerResponse<PaginatorDto<IEnumerable<SubscribedCreatorDto>>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "No active subscriptions found.",
                Data = new PaginatorDto<IEnumerable<SubscribedCreatorDto>>
                {
                    CurrentPage = subscriptions.CurrentPage,
                    PageSize = subscriptions.PageSize,
                    NumberOfPages = subscriptions.NumberOfPages,
                    PageItems = new List<SubscribedCreatorDto>() // Return an empty list if no creators found
                }
            };
        }

        return new ServerResponse<PaginatorDto<IEnumerable<SubscribedCreatorDto>>>
        {
            IsSuccessful = true,
            Data = new PaginatorDto<IEnumerable<SubscribedCreatorDto>>
            {
                CurrentPage = subscriptions.CurrentPage,
                PageSize = subscriptions.PageSize,
                NumberOfPages = subscriptions.NumberOfPages,
                PageItems = subscribedCreators
            },
            ResponseMessage = "List of subscribed creators retrieved successfully."
        };
    }

}

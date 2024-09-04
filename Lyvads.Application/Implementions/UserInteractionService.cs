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

namespace Lyvads.Application.Implementions;

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

    public async Task<Result> MakeRequestAsync(CreateRequestDto createRequestDto)
    {
        try
        {
            // Retrieve the current logged-in user from the authentication context
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (user == null)
                return new Error[] { new("User.Error", "User not found") };

            // Check if the user is a regular user (non-creator, non-admin)
            var isRegularUser = await _userManager.IsInRoleAsync(user, "RegularUser");
            if (!isRegularUser)
                return new Error[] { new("RegularUser.Error", "Regular User not found") };

            // Retrieve the creator
            var creator = await _creatorRepository.GetCreatorByIdAsync(createRequestDto.CreatorId);
            if (creator == null)
                return new Error[] { new("Creator.Error", "Creator not found") };

            string? domain = _configuration.GetValue<string>("Lyvads_Client_URL");

            if (string.IsNullOrEmpty(domain))
            {
                // Handle the case where the domain is null or empty
                _logger.LogError("Client URL is not configured.");
                return new Error[] { new("Configuration.Error", "Client URL is not configured.") };
            }

            Session session;

            if (createRequestDto.PaymentMethod == PaymentMethod.ATMCard)
            {
                var paymentDto = new PaymentDTO
                {
                    Amount = (int)(createRequestDto.Amount * 100), // Convert to cents
                    ProductName = "Request Payment", // You might want to adjust this
                    ReturnUrl = "/return-url" // Adjust this as needed
                };
                session = await _paymentGatewayService.CreateCardPaymentSessionAsync(paymentDto, domain);
            }
            else if (createRequestDto.PaymentMethod == PaymentMethod.Wallet)
            {
                var walletBalance = await _walletService.GetBalanceAsync(user.Id);

                if (walletBalance < createRequestDto.Amount)
                    return new Error[] { new("WalletBalance.Error", "Insufficient wallet balance.") };

                var result = await _walletService.DeductBalanceAsync(user.Id, createRequestDto.Amount);
                if (result)
                {
                    // Create and save the request
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
                        return Result.Success("Wallet payment successful and request created");
                    }
                    return new Error[] { new("CreateRequest.Error", "Failed to create request after wallet payment.") };
                }
                return new Error[] { new("WalletPayment.Error", "Failed to process wallet payment.") };
            }
            else if (createRequestDto.PaymentMethod == PaymentMethod.Online)
            {
                var paymentDto = new PaymentDTO
                {
                    Amount = (int)(createRequestDto.Amount * 100), // Convert to cents
                    ProductName = "Request Payment", // Adjust this as needed
                    ReturnUrl = "/return-url" // Adjust this as needed
                };
                session = await _paymentGatewayService.CreateOnlinePaymentSessionAsync(paymentDto, domain);
            }
            else
            {
                return new Error[] { new("Payment.Error", "Unsupported payment method.") };
            }

            if (session == null)
            {
                return new Error[] { new("Session.Error", "Failed to create payment session.") };
            }

            return Result.Success(session.Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while making request.");

            return new Error[] { new("Exception.Error", e.Message) };
        }
    }


    public async Task<Result> AddCommentAsync(string userId, string content)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Comment.Error", "User not found") };


        var comment = new Comment
        {
            UserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddCommentAsync(comment);
        return Result.Success();
    }

    public async Task<Result<CommentResponseDto>> EditCommentAsync(string commentId, string userId, string newContent)
    {
        _logger.LogInformation("User with ID: {UserId} is attempting to edit comment with ID: {CommentId}", userId, commentId);

        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Check if the comment exists
        var comment = _repository.GetAll<Comment>()
           .FirstOrDefault(x => x.UserId == user.Id);
        if (comment == null)
        {
            _logger.LogWarning("Comment with ID: {CommentId} not found.", commentId);
            return new Error[] { new("Comment.Error", "Comment not found.") };
        }

        // Ensure that the user is the owner of the comment
        if (comment.UserId != userId)
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to edit comment with ID: {CommentId}", userId, commentId);
            return new Error[] { new("Authorization.Error", "You are not authorized to edit this comment.") };
        }

        // Update the comment content
        comment.Content = newContent;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        _repository.Update(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment with ID: {CommentId} was successfully updated by user with ID: {UserId}", commentId, userId);

        var commentResponse = new CommentResponseDto
        {
            CommentId = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            CommentBy = comment.CommentBy
        };

        return Result<CommentResponseDto>.Success(commentResponse);
    }

    public async Task<Result> DeleteCommentAsync(string commentId, string userId)
    {
        _logger.LogInformation("User with ID: {UserId} is attempting to delete comment with ID: {CommentId}", userId, commentId);

        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Check if the comment exists
        var comment = _repository.GetAll<Comment>()
           .FirstOrDefault(x => x.UserId == user.Id);
        if (comment == null)
        {
            _logger.LogWarning("Comment with ID: {CommentId} not found.", commentId);
            return new Error[] { new("Comment.Error", "Comment not found.") };
        }

        // Ensure that the user is the owner of the comment
        if (comment.UserId != userId)
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to delete comment with ID: {CommentId}", userId, commentId);
            return new Error[] { new("Authorization.Error", "You are not authorized to delete this comment.") };
        }

        // Delete the comment
        _repository.Remove(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment with ID: {CommentId} was successfully deleted by user with ID: {UserId}", commentId, userId);

        return Result.Success();
    }

    public async Task<Result> LikeContentAsync(string userId, string contentId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Like.Error", "User not found") };
      
        var like = new Like
        {
            UserId = userId,
            ContentId = contentId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddLikeAsync(like);
        return Result.Success();
    }

    public async Task<Result> UnlikeContentAsync(string userId, string contentId)
    {
        // Check if the like exists for the user and content
        var like = await _userRepository.GetLikeAsync(userId, contentId);
        if (like == null)
        {
            _logger.LogWarning("Like not found for UserId: {UserId} and ContentId: {ContentId}", userId, contentId);
            return new Error[] { new("Unlike.Error", "Like not found.") };
        }

        // Remove the like
        await _userRepository.RemoveLikeAsync(like);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User with ID: {UserId} unliked content with ID: {ContentId}", userId, contentId);

        return Result.Success();
    }    


    public async Task<Result> FundWalletAsync(string userId, decimal amount)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Wallet.Error", "User not found") };

        await _userRepository.UpdateWalletBalanceAsync(userId, amount);
        return Result.Success();
    }

    public async Task<Result> CreateRequestAsync(CreateRequestDto createRequestDto)
    {
        _logger.LogInformation("******* Inside the CreateRequestAsync Method ********");

        // Retrieve the user and creator
        var user = await _userRepository.GetUserByIdAsync(createRequestDto.UserId);
        var creator = await _creatorRepository.GetCreatorByIdAsync(createRequestDto.CreatorId);

        if (user == null)
            return new Error[] { new("Request.Error", "User not found") };

        if (creator == null)
            return new Error[] { new("Request.Error", "Creator not found") };

        // Create and save the request
        var request = new Request
        {
            Type = createRequestDto.Type,
            Script = createRequestDto.Script,
            CreatorId = createRequestDto.CreatorId,
            UserId = createRequestDto.UserId,
            RequestType = createRequestDto.RequestType,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await _repository.Add(request);

        // Handle payment logic
        if (createRequestDto.PaymentMethod == PaymentMethod.Wallet)
        {
            var paymentResult = await FundWalletAsync(user.Id, createRequestDto.Amount);
            if (!paymentResult.IsSuccess)
                return paymentResult; // Propagate payment errors
        }
        else if (createRequestDto.PaymentMethod == PaymentMethod.Online)
        {
            var paymentResult = await _paymentGatewayService.ProcessPaymentAsync(createRequestDto.Amount, "usd", createRequestDto.Source ?? string.Empty, "Video request payment");
            if (!paymentResult.IsSuccess)
                return paymentResult; // Propagate payment errors
        }
        else if (createRequestDto.PaymentMethod == PaymentMethod.ATMCard)
        {
            // Similar logic for ATM card if it requires separate handling
        }

        return Result.Success();
    }

    

}

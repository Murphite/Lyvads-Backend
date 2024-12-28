
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Utilities;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Interfaces;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementations;

public class RegularUserService : IRegularUserService
{
    private readonly IRepository _repository;
    private readonly IRegularUserRepository _regularUserRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegularUserService> _logger;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;

    public RegularUserService(IRepository repository,
        IRegularUserRepository regularUserrepository,
    IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPaymentGatewayService paymentGatewayService,
        UserManager<ApplicationUser> userManager,
        ILogger<RegularUserService> logger, 
        IEmailService emailService,
        IVerificationService verificationService)
    {
        _repository = repository;
        _regularUserRepository = regularUserrepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _verificationService = verificationService;
    }


    public async Task<ServerResponse<PaginatorDto<IEnumerable<RegularUserDto>>>> GetRegularUsers(PaginationFilter paginationFilter)
    {
        _logger.LogInformation("******* Inside the GetRegularUsers Method ********");

        if (paginationFilter == null)
        {
            return new ServerResponse<PaginatorDto<IEnumerable<RegularUserDto>>>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid pagination parameters",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Pagination filter cannot be null."
                }
            };
        }

        try
        {
            var regularUsersQuery = _regularUserRepository.GetRegularUsers()
                .Select(u => new RegularUserDto
                {
                    UserId = u.Id,
                    FullName = u.ApplicationUser!.FullName, 
                    AppUserName = u.ApplicationUser.AppUserName,
                    Email = u.ApplicationUser.Email,
                    ProfilePictureUrl = u.ApplicationUser.ImageUrl,
                    CreatedAt = u.ApplicationUser.CreatedAt,
                })
                .AsQueryable();

            var paginatedResult = await Pagination.PaginateAsync(regularUsersQuery, paginationFilter);

            if (paginatedResult.PageItems == null || !paginatedResult.PageItems.Any())
            {
                return new ServerResponse<PaginatorDto<IEnumerable<RegularUserDto>>>
                {
                    IsSuccessful = false,
                    ResponseCode = "404",
                    ResponseMessage = "No regular users found.",
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "No users found matching the criteria."
                    }
                };
            }

            return new ServerResponse<PaginatorDto<IEnumerable<RegularUserDto>>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Regular users retrieved successfully.",
                Data = paginatedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regular users");
            return new ServerResponse<PaginatorDto<IEnumerable<RegularUserDto>>>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while retrieving regular users.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal server error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    public async Task<Result<RegularUserProfileResponseDto>> UpdateUserProfileAsync(UpdateRegularUserProfileDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Find the creator associated with the user
        var creator = _repository.GetAll<Creator>()
            .FirstOrDefault(x => x.ApplicationUserId == user.Id);

        if (creator == null)
            return new Error[] { new("creators.Error", "Creator Not Found") };

        // Split FullName if provided
        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var nameParts = dto.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = nameParts.Length > 0 ? nameParts[0] : user.FirstName;
            user.LastName = nameParts.Length > 1 ? nameParts[1] : user.LastName;
        }
        else
        {
            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
        }
        user.Bio = dto.Bio ?? user.Bio;
        user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
        user.UserName = dto.Username ?? user.UserName;
        user.Location = dto.Location ?? user.Location;
        user.Email = dto.Email ?? user.Email;
        user.Occupation = dto.Occupation ?? user.Occupation;

        // Handle email verification if the email is being updated
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            // Send verification code to the new email address
            var verificationCode = GenerateVerificationCode();

            var emailBody = $"Your verification code is {verificationCode}";
            var emailResult = await _emailService.SendEmailAsync(dto.Email, "Email Verification", emailBody);
            if (!emailResult)
                return new Error[] { new("Registration.Error", "Error sending verification email") };

            // Store the verification code and email in a temporary storage
            await _verificationService.SaveVerificationCode(dto.Email, verificationCode);

            // Set the email in EmailContext for future use
            EmailContext.VerifiedEmail = dto.Email;

            // Ask the user to input the verification code
            var inputCode = dto.VerificationCode;

            // Verify the code
            if (verificationCode != inputCode)
            {
                return new Error[] { new("Verification.Error", "Invalid verification code.") };
            }

            // Update the email if verification succeeds
            user.Email = dto.Email;
        }

        // Save the updates to the database
        _repository.Update(creator);
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var regularUserProfileResponse = new RegularUserProfileResponseDto
        {
            FullName = user.FullName,
            Bio = user.Bio,
            Username = user.UserName,
            PhoneNumber = user.PhoneNumber,
            Location = user.Location,
            Occupation = user.Occupation,
        };

        return Result<RegularUserProfileResponseDto>.Success(regularUserProfileResponse);
    }





    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }

    public static class EmailContext
    {
        public static string VerifiedEmail { get; set; } = string.Empty;
    }

    
}

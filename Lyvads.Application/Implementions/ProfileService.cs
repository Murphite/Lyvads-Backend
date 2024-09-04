using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Entities;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Interfaces;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.AuthDtos;

namespace Lyvads.Application.Implementions;

public class ProfileService : IProfileService
{
    private readonly IRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProfileService> _logger;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;

    public ProfileService(IRepository repository,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<ProfileService> logger,
        IEmailService emailService,
        IVerificationService verificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _verificationService = verificationService;
    }



    public async Task<Result<EditProfileResponseDto>> EditProfileAsync(EditProfileDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Split FullName if provided
        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var nameParts = dto.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = nameParts.Length > 0 ? nameParts[0] : user.FirstName;
            user.LastName = nameParts.Length > 1 ? nameParts[1] : user.LastName;
        }
        else
        {
            user.FirstName = user.FirstName;
            user.LastName = user.LastName;
        }

        user.Bio = dto.Bio ?? user.Bio;
        user.UserName = dto.Username ?? user.UserName;

        // Save the updates to the database
        _repository.Update(user);
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var editProfileResponse = new EditProfileResponseDto
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Bio = user.Bio,
            Username = user.UserName,
        };

        return Result<EditProfileResponseDto>.Success(editProfileResponse);
    }

    public async Task<Result<UpdateProfilePicResponseDto>> UpdateProfilePictureAsync(string userId, string newProfilePictureUrl)
    {
        _logger.LogInformation("User with ID: {UserId} is updating his profile picture", userId);

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        user.ImageUrl = newProfilePictureUrl;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return new Error[] { new("Update.Error", "Profile picture update not successful") };

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var updateProfilePicResponseDto = new UpdateProfilePicResponseDto
        {
            UserId = user.Id,
            NewProfilePictureUrl = user.ImageUrl,
        };

        return Result<UpdateProfilePicResponseDto>.Success(updateProfilePicResponseDto);
    }

    public async Task<Result<UpdateEmailResponseDto>> InitiateEmailUpdateAsync(string userId, string newEmail)
    {
        _logger.LogInformation("******* Inside the InitiateEmailUpdateAsync Method ********");

        // Check if the new email already exists in the system
        var emailExist = await _userManager.FindByEmailAsync(newEmail);
        if (emailExist != null)
            return new Error[] { new("UpdateEmail.Error", "Email already in use") };

        // Retrieve the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new Error[] { new("UpdateEmail.Error", "User not found") };

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(newEmail, "Email Verification", emailBody);
        if (!emailResult)
            return new Error[] { new("UpdateEmail.Error", "Error sending verification email") };

        // Store the verification code and new email in temporary storage
        await _verificationService.SaveVerificationCode(newEmail, verificationCode);

        // Create the response DTO
        var updateEmailResponse = new UpdateEmailResponseDto
        {
            Email = newEmail,
            VerificationCode = verificationCode
        };

        return Result<UpdateEmailResponseDto>.Success(updateEmailResponse);
    }

    public async Task<Result<EmailVerificationResponseDto>> VerifyEmailUpdateAsync(string userId, string verificationCode)
    {
        _logger.LogInformation($"******* Inside the VerifyEmailUpdateAsync Method ********");

        // Retrieve the new email using the verification code
        var newEmail = await _verificationService.GetEmailByVerificationCode(verificationCode);
        if (newEmail == null)
            return new Error[] { new("Verification.Error", "Invalid verification code") };

        var isCodeValid = await _verificationService.ValidateVerificationCode(newEmail, verificationCode);
        if (!isCodeValid)
            return new Error[] { new("Verification.Error", "Invalid verification code") };

        // Retrieve the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new Error[] { new("User.Error", "User not found") };

        // Update the user's email
        user.Email = newEmail;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return new Error[] { new("UpdateEmail.Error", "Email update not successful") };

        await _unitOfWork.SaveChangesAsync();

        // Create the response DTO
        var verificationResponse = new EmailVerificationResponseDto
        {
            Email = newEmail,
            IsVerified = true,
            Message = "Email updated and verified successfully."
        };

        return Result<EmailVerificationResponseDto>.Success(verificationResponse);
    }

    public async Task<Result<UpdateLocationResponseDto>> UpdateLocationAsync(UpdateLocationDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Update user's location properties
        user.Location = dto.Country ?? user.Location;

        // Save the updates to the database
        _repository.Update(user);
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return new Error[] { new("Update.Error", "Failed to update location") };

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var updateLocationResponse = new UpdateLocationResponseDto
        {
            Country = user.Location,
        };

        return Result<UpdateLocationResponseDto>.Success(updateLocationResponse);
    }

    public async Task<Result<UpdatePhoneNumberResponseDto>> UpdatePhoneNumberAsync(UpdatePhoneNumberDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Update user's phone number
        user.PhoneNumber = dto.PhoneNumber ?? string.Empty;

        // Save the updates to the database
        _repository.Update(user);
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return new Error[] { new("Update.Error", "Failed to update phone number") };

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var updatePhoneNumberResponse = new UpdatePhoneNumberResponseDto
        {
            PhoneNumber = user.PhoneNumber,
        };

        return Result<UpdatePhoneNumberResponseDto>.Success(updatePhoneNumberResponse);
    }






    public class UpdatePhoneNumberResponseDto
    {
        public string? PhoneNumber { get; set; }
    }

    public class UpdatePhoneNumberDto
    {
        public string? PhoneNumber { get; set; }
    }

    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }

    public class UpdateLocationDto
    {
        public string? Country { get; set; }
    }

    public class UpdateLocationResponseDto
    {
        public string? Country { get; set; }
    }


}

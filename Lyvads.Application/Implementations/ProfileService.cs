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
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Http;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Shared.DTOs;

namespace Lyvads.Application.Implementations;

public class ProfileService : IProfileService
{
    private readonly IRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProfileService> _logger;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;
    private readonly IMediaService _mediaService;

    public ProfileService(IRepository repository,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<ProfileService> logger,
        IEmailService emailService,
        IVerificationService verificationService,
        IMediaService mediaService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _verificationService = verificationService;
        _mediaService = mediaService;
    }



    public async Task<ServerResponse<EditProfileResponseDto>> EditProfileAsync(EditProfileDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new ServerResponse<EditProfileResponseDto>
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
            AppUsername = user.AppUserName,
        };
        
        return new ServerResponse<EditProfileResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Profile Edited successfully.",
            Data = editProfileResponse
        };
    }

    public async Task<ServerResponse<UserProfileDto>> GetProfileAsync(string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return new ServerResponse<UserProfileDto>
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

        // Prepare the response DTO
        var userProfileDto = new UserProfileDto
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Bio = user.Bio!,
            Username = user.UserName!,
            AppUsername = user.AppUserName!,
            Email = user.Email!,
            ProfilePic = user.ImageUrl!,
            PhoneNumber = user.PhoneNumber!,
            Location = user.Location!
        };

        return new ServerResponse<UserProfileDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "User profile retrieved successfully.",
            Data = userProfileDto
        };
    }

    public async Task<ServerResponse<UpdateProfilePicResponseDto>> UploadProfilePictureAsync(string userId, IFormFile newProfilePicture)
    {
        _logger.LogInformation("User with ID: {UserId} is updating their profile picture", userId);

        string folderName = "user_profile_pictures";  // Folder for uploaded images

        // Check if user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<UpdateProfilePicResponseDto>
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

        // Upload the profile picture to Cloudinary (or any other service)
        var uploadResult = await _mediaService.UploadImageAsync(newProfilePicture, folderName);

        if (uploadResult["Code"] != "200")
        {
            _logger.LogError("Failed to upload profile picture for user with ID: {UserId}", userId);
            return new ServerResponse<UpdateProfilePicResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to upload profile picture.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Failed to upload profile picture."
                }
            };
        }

        // Update profile picture URL
        user.ImageUrl = uploadResult["Url"];
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return new ServerResponse<UpdateProfilePicResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Profile picture update was not successful.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Profile picture update was not successful."
                }
            };
        }

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response
        var updateProfilePicResponseDto = new UpdateProfilePicResponseDto
        {
            UserId = user.Id,
            NewProfilePictureUrl = user.ImageUrl,
        };

        return new ServerResponse<UpdateProfilePicResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Profile picture updated successfully.",
            Data = updateProfilePicResponseDto
        };
    }

    public async Task<ServerResponse<UpdateProfilePicResponseDto>> UpdateProfilePictureAsync(string userId, IFormFile newProfilePicture)
    {
        _logger.LogInformation("User with ID: {UserId} is updating their profile picture", userId);

        // Define the folder name here, so it's not passed from the controller
        string folderName = "user_profile_pictures";  // You can adjust this based on your requirements

        // Check if the user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<UpdateProfilePicResponseDto>
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

        // Upload the profile picture to Cloudinary
        var uploadResult = await _mediaService.UploadImageAsync(newProfilePicture, folderName);

        if (uploadResult["Code"] != "200")
        {
            _logger.LogError("Failed to upload profile picture for user with ID: {UserId}", userId);
            return new ServerResponse<UpdateProfilePicResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to upload profile picture.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Failed to upload profile picture."
                }
            };
        }

        var newProfilePictureUrl = uploadResult["Url"];

        // Update the user's profile picture URL
        user.ImageUrl = newProfilePictureUrl;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return new ServerResponse<UpdateProfilePicResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Profile picture update was not successful.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Profile picture update was not successful."
                }
            };
        }

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var updateProfilePicResponseDto = new UpdateProfilePicResponseDto
        {
            UserId = user.Id,
            NewProfilePictureUrl = user.ImageUrl,
        };

        return new ServerResponse<UpdateProfilePicResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Profile picture updated successfully.",
            Data = updateProfilePicResponseDto
        };
    }

    public async Task<ServerResponse<UpdateEmailResponseDto>> InitiateEmailUpdateAsync(string userId, string newEmail)
    {
        _logger.LogInformation("******* Inside the InitiateEmailUpdateAsync Method ********");

        // Check if the new email already exists in the system
        var emailExist = await _userManager.FindByEmailAsync(newEmail);
        if (emailExist != null)
            return new ServerResponse<UpdateEmailResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Email already in use.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Email already in use.",
                    ResponseDescription = "Email already in use, try out another!."
                }
            };

        // Retrieve the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new ServerResponse<UpdateEmailResponseDto>
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

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(newEmail, "Email Verification", emailBody);
        if (!emailResult)
            //return new Error[] { new("UpdateEmail.Error", "Error sending verification email") };
            return new ServerResponse<UpdateEmailResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Error sending verification email.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Error sending verification email."
                }
            };

        // Store the verification code and new email in temporary storage
        await _verificationService.SaveVerificationCode(newEmail, verificationCode);

        // Create the response DTO
        var updateEmailResponse = new UpdateEmailResponseDto
        {
            Email = newEmail,
            VerificationCode = verificationCode
        };

        return new ServerResponse<UpdateEmailResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Verification code email sent successfully.",
            Data = updateEmailResponse
        };
    }

    public async Task<ServerResponse<EmailVerificationResponseDto>> VerifyEmailUpdateAsync(string userId, string verificationCode)
    {
        _logger.LogInformation($"******* Inside the VerifyEmailUpdateAsync Method ********");

        // Retrieve the new email using the verification code
        var newEmail = await _verificationService.GetEmailByVerificationCode(verificationCode);
        if (newEmail == null)
            //eturn new Error[] { new("Verification.Error", "Invalid verification code") };
            return new ServerResponse<EmailVerificationResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Invalid verification code.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Invalid verification code."
                }
            };


        var isCodeValid = await _verificationService.ValidateVerificationCode(newEmail, verificationCode);
        if (!isCodeValid)
            return new ServerResponse<EmailVerificationResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Invalid verification code.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Invalid verification code."
                }
            };

        // Retrieve the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new ServerResponse<EmailVerificationResponseDto>
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

        // Update the user's email
        user.Email = newEmail;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return new ServerResponse<EmailVerificationResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Email update was not successful.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Email Update was not successful."
                }
            };

        await _unitOfWork.SaveChangesAsync();

        // Create the response DTO
        var verificationResponse = new EmailVerificationResponseDto
        {
            Email = newEmail,
            IsVerified = true,
            Message = "Email updated and verified successfully."
        };

        return new ServerResponse<EmailVerificationResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Email updated and verified successfully.",
            Data = verificationResponse
        };
    }

    public async Task<ServerResponse<UpdateLocationResponseDto>> UpdateLocationAsync(UpdateLocationDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new ServerResponse<UpdateLocationResponseDto>
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

        // Update user's location properties
        user.Location = dto.Country ?? user.Location;

        // Save the updates to the database
        _repository.Update(user);
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return new ServerResponse<UpdateLocationResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Failed to update location.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Failed to update location."
                }
            };

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var updateLocationResponse = new UpdateLocationResponseDto
        {
            Country = user.Location,
        };

        return new ServerResponse<UpdateLocationResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Location updated successfully.",
            Data = updateLocationResponse
        };
    }

    public async Task<ServerResponse<UpdatePhoneNumberResponseDto>> UpdatePhoneNumberAsync(UpdatePhoneNumberDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new ServerResponse<UpdatePhoneNumberResponseDto>
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

        // Update user's phone number
        user.PhoneNumber = dto.PhoneNumber ?? string.Empty;

        // Save the updates to the database
        _repository.Update(user);
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return new ServerResponse<UpdatePhoneNumberResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Failed to update phone number.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Failed to update phone number"
                }
            };

        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var updatePhoneNumberResponse = new UpdatePhoneNumberResponseDto
        {
            PhoneNumber = user.PhoneNumber,
        };

        return new ServerResponse<UpdatePhoneNumberResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Phone number updated successfully.",
            Data = updatePhoneNumberResponse
        };
    }

    public async Task<ServerResponse<bool>> ValidatePasswordAsync(string email, ValidatePasswordDto validatePasswordDto)
    {
        _logger.LogInformation("******* Inside the ValidatePasswordAsync Method ********");

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            _logger.LogWarning("User not found for email: {email}", email);
            return new ServerResponse<bool>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "User not found"
                }
            };
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, validatePasswordDto.Password!);

        if (!isValidPassword)
        {
            _logger.LogWarning("Invalid password for user: {email}", user.Email);
            return new ServerResponse<bool>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "Incorrect password"
                }
            };
        }

        _logger.LogInformation("Password validation successful for user: {email}", user.Email);

        return new ServerResponse<bool>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Password is correct",
            Data = true
        };
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

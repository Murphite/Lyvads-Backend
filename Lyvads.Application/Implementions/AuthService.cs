using Lyvads.Application.Dtos;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Interfaces;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Web;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Constants;
using Lyvads.Application.Dtos.AuthDtos;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using static Lyvads.Application.Implementions.AuthService;
using Microsoft.EntityFrameworkCore;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Application.Dtos.CreatorDtos;

namespace Lyvads.Application.Implementions;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;
    private readonly IAdminRepository _adminRepository;
    private readonly IRegularUserRepository _regularUserRepository;
    private readonly ICreatorRepository _creatorRepository;

    public AuthService(UserManager<ApplicationUser> userManager, IRepository repository, IJwtService jwtService,
        IEmailService emailService, IVerificationService verificationService, IConfiguration configuration, IUnitOfWork unitOfWork, 
        ILogger<AuthService> logger, IAdminRepository adminRepository, IRegularUserRepository regularUserRepository, ICreatorRepository creatorRepository)
    {
        _userManager = userManager;
        _repository = repository;
        _jwtService = jwtService;
        _emailService = emailService;
        _verificationService = verificationService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _adminRepository = adminRepository;
        _regularUserRepository = regularUserRepository;
        _creatorRepository = creatorRepository;
    }

    public async Task<Result<RegistrationResponseDto>> InitiateRegistration(string email)
    {
        _logger.LogInformation("******* Inside the InitiateRegistration Method ********");

        var emailExist = await _userManager.FindByEmailAsync(email);
        if (emailExist != null)
            return new Error[] { new("Registration.Error", "Email already exists") };

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(email, "Email Verification", emailBody);
        if (!emailResult)
            return new Error[] { new("Registration.Error", "Error sending verification email") };

        // Store the verification code and email in a temporary storage
        await _verificationService.SaveVerificationCode(email, verificationCode);

        // Set the email in EmailContext for future use
        EmailContext.VerifiedEmail = email;

        // Create the response DTO
        var registrationResponse = new RegistrationResponseDto
        {
            Email = email,
            VerificationCode = verificationCode
        };

        return Result<RegistrationResponseDto>.Success(registrationResponse);
    }

    public async Task<Result<EmailVerificationResponseDto>> VerifyEmail(string verificationCode)
    {
        _logger.LogInformation($"******* Inside the VerifyEmail Method ********");

        // Retrieve the stored email using the verification code
        var email = await _verificationService.GetEmailByVerificationCode(verificationCode);
        if (email == null)
            return new Error[] { new("Verification.Error", "Invalid verification code") };

        var isCodeValid = await _verificationService.ValidateVerificationCode(email, verificationCode);
        if (!isCodeValid)
            return new Error[] { new("Verification.Error", "Invalid verification code") };

        // Mark email as verified
        await _verificationService.MarkEmailAsVerified(email);

        // Create the response DTO
        var verificationResponse = new EmailVerificationResponseDto
        {
            Email = email,
            IsVerified = true,
            Message = "Email verified. Proceed with registration."
        };

        return Result<EmailVerificationResponseDto>.Success(verificationResponse);
    }

    public async Task<Result<RegisterUserResponseDto>> RegisterAdmin(RegisterAdminDto registerAdminDto)
    {
        _logger.LogInformation("******* Inside the RegisterAdmin Method ********");

        // Retrieve the verified email from the context
        var verifiedEmail = EmailContext.VerifiedEmail;
        if (string.IsNullOrEmpty(verifiedEmail))
            return new Error[] { new("Verification.Error", "Email not verified") };

        // Ensure password and confirm password match
        if (registerAdminDto.Password != registerAdminDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        // Split the full name
        var names = registerAdminDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Create ApplicationUser
        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = verifiedEmail,
            Username = registerAdminDto.Username,
            Email = verifiedEmail,
            PhoneNumber = registerAdminDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            WalletId = GenerateWalletId(),
            PublicId = Guid.NewGuid().ToString(),
        };

        // Create Admin entity and associate the ApplicationUser
        var admin = new Admin
        {
            UserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerAdminDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.Admin);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Assuming you have a repository or context to save the Admin entity
        await _adminRepository.AddAsync(admin);

        // Mark email as verified
        await _verificationService.MarkEmailAsVerified(verifiedEmail);

        // Clear the email from the context after successful registration
        EmailContext.VerifiedEmail = string.Empty;

        var registerUserResponse = new RegisterUserResponseDto
        {
            UserId = applicationUser.Id,
            Username = applicationUser.Username,
            Email = applicationUser.Email,
            Role = RolesConstant.RegularUser,  // Include the role here
            Message = "Registration successful. Verification is pending until the confirmation of account."
        };

        return Result<RegisterUserResponseDto>.Success(registerUserResponse);
    }

    public async Task<Result<RegisterUserResponseDto>> RegisterUser(RegisterUserDto registerUserDto)
    {
        _logger.LogInformation("******* Inside the RegisterUser Method ********");

        var verifiedEmail = EmailContext.VerifiedEmail;
        if (string.IsNullOrEmpty(verifiedEmail))
            return new Error[] { new("Verification.Error", "Email not verified") };

        if (registerUserDto.Password != registerUserDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        var names = registerUserDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = verifiedEmail,
            Username = registerUserDto.Username,
            Email = verifiedEmail,
            PhoneNumber = registerUserDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            WalletId = GenerateWalletId(),
            PublicId = Guid.NewGuid().ToString(),
        };

        var regularUser = new RegularUser
        {
            UserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerUserDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        var roleResult = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.RegularUser);
        if (!roleResult.Succeeded)
            return roleResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        await _regularUserRepository.AddAsync(regularUser);
        await _verificationService.MarkEmailAsVerified(verifiedEmail);
        EmailContext.VerifiedEmail = string.Empty;

        // Create the response DTO including the role
        var registerUserResponse = new RegisterUserResponseDto
        {
            UserId = applicationUser.Id,
            Username = applicationUser.Username,
            Email = applicationUser.Email,
            Role = RolesConstant.RegularUser,
            Message = "Registration successful. Verification is pending until the confirmation of account."
        };

        return Result<RegisterUserResponseDto>.Success(registerUserResponse);
    }

    public async Task<Result<RegisterUserResponseDto>> RegisterCreator(RegisterCreatorDto registerCreatorDto)
    {
        _logger.LogInformation("******* Inside the RegisterAdmin Method ********");

        // Retrieve the verified email from the context
        var verifiedEmail = EmailContext.VerifiedEmail;
        if (string.IsNullOrEmpty(verifiedEmail))
            return new Error[] { new("Verification.Error", "Email not verified") };

        // Ensure password and confirm password match
        if (registerCreatorDto.Password != registerCreatorDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        // Split the full name
        var names = registerCreatorDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Create ApplicationUser
        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = verifiedEmail,
            Username = registerCreatorDto.Username,
            Email = verifiedEmail,
            PhoneNumber = registerCreatorDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            WalletId = GenerateWalletId(),
            PublicId = Guid.NewGuid().ToString(),
        };

        // Create Creator entity and associate the ApplicationUser
        var creator = new Creator
        {
            UserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerCreatorDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.Creator);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Assuming you have a repository or context to save the Admin entity
        await _creatorRepository.AddAsync(creator);

        // Mark email as verified
        await _verificationService.MarkEmailAsVerified(verifiedEmail);

        // Clear the email from the context after successful registration
        EmailContext.VerifiedEmail = string.Empty;

        var registerUserResponse = new RegisterUserResponseDto
        {
            UserId = applicationUser.Id,
            Username = applicationUser.Username,
            Email = applicationUser.Email,
            Role = RolesConstant.Creator,
            Message = "Registration successful. Verification is pending until the confirmation of account."
        };

        return Result<RegisterUserResponseDto>.Success(registerUserResponse);
    }

    public async Task<Result<LoginResponseDto>> Login(LoginUserDto loginUserDto)
    {
        _logger.LogInformation($"******* Inside the Login Method ********");

        var user = await _userManager.FindByEmailAsync(loginUserDto.Email);

        if (user is null)
            return new Error[] { new("Auth.Error", "email or password not correct") };

        var isValidUser = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);

        if (!isValidUser)
            return new Error[] { new("Auth.Error", "email or password not correct") };

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        var email = user.Email ?? string.Empty;
        var fullName = user.FullName ?? string.Empty;

        return new LoginResponseDto(token, fullName, roles, email);
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation($"******* Inside the ResetPassword Method ********");

        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user is null)
            return new Error[] { new("Auth.Error", "No user found with the provided email") };

        var resetPasswordResult =
            await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

        if (!resetPasswordResult.Succeeded)
            return resetPasswordResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        return Result.Success("Password reset successfully.");
    }

    public async Task<Result> ForgotPassword(ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation($"******* Inside the ForgotPassword Method ********");

        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

        if (user == null)
            return new Error[] { new("Auth.Error", "No user found with the provided email") };

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink =
            $"{_configuration["ResetPasswordUrl"]}?email={HttpUtility.UrlEncode(user.Email)}&token={HttpUtility.UrlEncode(token)}";

        const string emailSubject = "Your New Password";

        var emailBody = $"Hello {user.FullName}, click this link to reset your password: {resetLink}.";

        var isSuccessful = await _emailService.SendEmailAsync(resetPasswordDto.Email, emailSubject, emailBody);
        if (!isSuccessful)
            return new Error[] { new("Auth.Error", "Error occured while sending reset password email") };

        return Result.Success();
    }

    public async Task<Result> ConfirmEmail(string email, string token)
    {
        _logger.LogInformation($"******* Inside the ConfirmEmail Method ********");

        _logger.LogInformation($"******* Inside the Confirm Method ********");

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return new Error[] { new("Auth.Error", "User not found") };

        var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, token);

        if (!confirmEmailResult.Succeeded)
        {
            return Result.Failure(confirmEmailResult.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
        }

        user.EmailConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result.Failure(updateResult.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
        }

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordDto model)
    {
        _logger.LogInformation($"******* Inside the ChangePassword Method ********");

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
            return new Error[] { new("Auth.Error", "email not correct") };

        if (!await _userManager.CheckPasswordAsync(user, model.OldPassword))
            return new Error[] { new("Auth.Error", "password not correct") };

        if (model.NewPassword != model.ConfirmPassword)
            return new Error[] { new("Auth.Error", "Newpassword and Confirmpassword must match") };

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        return Result.Success("Password changed successfully.");
    }    



    public async Task<Result> ConfirmRegistration(string userId)
    {
        _logger.LogInformation($"******* Inside the ConfirmRegistration Method ********");

        // Find the user by userId
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Verification.Error", "User not found") };

        // Mark the user as verified
        user.IsVerified = true;
        user.VerificationCode = null; // Clear the verification code

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Send confirmation email to user
        var confirmationBody = "Your account has been verified and confirmed by admins.";
        var confirmationEmailResult = await _emailService.SendEmailAsync(user.Email ?? string.Empty, "Account Confirmation", confirmationBody);
        if (!confirmationEmailResult)
            return new Error[] { new("Verification.Error", "Error sending confirmation email") };

        return Result.Success();
    }
    






    public static class EmailContext
    {
        public static string VerifiedEmail { get; set; } = string.Empty;
    }

    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }

    public string GenerateWalletId()
    {
        return Guid.NewGuid().ToString();
    }

}
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

namespace Lyvads.Application.Implementions;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, IRepository repository, IJwtService jwtService,
        IEmailService emailService, IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _repository = repository;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> RegisterUser(RegisterUserDto registerUserDto)
    {
        _logger.LogInformation($"******* Inside the RegisterUser Method ********");

        // Ensure email is unique
        var emailExist = await _userManager.FindByEmailAsync(registerUserDto.Email);
        if (emailExist != null)
            return new Error[] { new("Registration.Error", "Email already exists") };

        // Split the full name
        var names = registerUserDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        var user = new ApplicationUser
        {
            Username = registerUserDto.Email,
            Email = registerUserDto.Email,
            FirstName = firstName,
            LastName = lastName,
            VerificationCode = verificationCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Ensure password and confirm password match
        if (registerUserDto.Password != registerUserDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        var result = await _userManager.CreateAsync(user, registerUserDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(user.Email, "Email Verification", emailBody);
        if (!emailResult)
            return new Error[] { new("Registration.Error", "Error sending verification email") };

        // Call method to verify email with the received code
        var verifyResult = await VerifyEmail(user.Email, verificationCode);
        if (!verifyResult.IsSuccess)
            return verifyResult.Errors.ToArray();

        result = await _userManager.AddToRoleAsync(user, RolesConstant.RegularUser);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();


        return Result.Success("Registration successful. Verification is pending until the confirmation of account.");
    }

    public async Task<Result> RegisterCreator(RegisterCreatorDto registerCreatorDto)
    {
        _logger.LogInformation($"******* Inside the RegisterCreator Method ********");

        var emailExist = await _userManager.FindByEmailAsync(registerCreatorDto.Email);
        if (emailExist != null)
            return new Error[] { new("Registration.Error", "Email already exists") };

        // Split the full name
        var names = registerCreatorDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        var user = new Creator
        {
            Username = registerCreatorDto.Username,
            Email = registerCreatorDto.Email,
            UserName = registerCreatorDto.Email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = registerCreatorDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            VerificationCode = verificationCode,
        };

        // Ensure password and confirm password match
        if (registerCreatorDto.Password != registerCreatorDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        var result = await _userManager.CreateAsync(user, registerCreatorDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(user.Email, "Email Verification", emailBody);
        if (!emailResult)
            return new Error[] { new("Registration.Error", "Error sending verification email") };

        // Call method to verify email with the received code
        var verifyResult = await VerifyEmail(user.Email, verificationCode);
        if (!verifyResult.IsSuccess)
            return verifyResult.Errors.ToArray();

        result = await _userManager.AddToRoleAsync(user, RolesConstant.Creator);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();


        return Result.Success("Registration successful. Verification is pending until the confirmation of account.");
    }

    public async Task<Result> RegisterAdmin(RegisterAdminDto registerAdminDto)
    {
        _logger.LogInformation($"******* Inside the RegisterAdmin Method ********");

        var emailExist = await _userManager.FindByEmailAsync(registerAdminDto.Email);

        if (emailExist != null)
            return new Error[] { new("Registration.Error", "email already exist") };

        // Split the full name
        var names = registerAdminDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        var user = new Admin
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = registerAdminDto.Email,
            Email = registerAdminDto.Email,
            PhoneNumber = registerAdminDto.PhoneNumber,
            Username = registerAdminDto.Email,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            VerificationCode = verificationCode,
        };

        // Ensure password and confirm password match
        if (registerAdminDto.Password != registerAdminDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        var result = await _userManager.CreateAsync(user, registerAdminDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(user.Email, "Email Verification", emailBody);
        if (!emailResult)
            return new Error[] { new("Registration.Error", "Error sending verification email") };

        // Call method to verify email with the received code
        var verifyResult = await VerifyEmail(user.Email, verificationCode);
        if (!verifyResult.IsSuccess)
            return verifyResult.Errors.ToArray();

        result = await _userManager.AddToRoleAsync(user, RolesConstant.RegularUser);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();


        return Result.Success("Registration successful. Verification is pending until the confirmation of account.");
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

        return new LoginResponseDto(token);
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

        return Result.Success();
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

        return Result.Success();
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
        var confirmationEmailResult = await _emailService.SendEmailAsync(user.Email, "Account Confirmation", confirmationBody);
        if (!confirmationEmailResult)
            return new Error[] { new("Verification.Error", "Error sending confirmation email") };

        return Result.Success();
    }


    // Backend method to verify email with the received code
    public async Task<Result> VerifyEmail(string email, string verificationCode)
    {
        _logger.LogInformation($"******* Inside the VerifyEmail Method ********");

        // Find the user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new Error[] { new("Verification.Error", "User not found") };

        // Verify the entered verification code
        if (user.VerificationCode != verificationCode)
            return new Error[] { new("Verification.Error", "Invalid verification code") };

        // Clear the verification code after successful verification
        user.VerificationCode = null;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        return Result.Success();
    }

    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }
}
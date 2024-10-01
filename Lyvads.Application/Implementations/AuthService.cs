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
using Lyvads.Domain.Responses;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Web.Helpers;

namespace Lyvads.Application.Implementations;

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
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly IEmailContext _emailContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthService(UserManager<ApplicationUser> userManager, 
        IRepository repository, 
        IJwtService jwtService,
        IEmailService emailService, 
        IVerificationService verificationService, 
        IConfiguration configuration, 
        IUnitOfWork unitOfWork, 
        ILogger<AuthService> logger, 
        IAdminRepository adminRepository, 
        IRegularUserRepository regularUserRepository,
        ICreatorRepository creatorRepository, 
        ISuperAdminRepository superAdminRepository,
        IEmailContext emailContext,
        IHttpContextAccessor httpContextAccessor)
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
        _superAdminRepository = superAdminRepository;
        _emailContext = emailContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServerResponse<RegistrationResponseDto>> InitiateRegistration(string email)
    {
        _logger.LogInformation("******* Inside the InitiateRegistration Method ********");
        email = email.ToLower();

        // Check if the email format is valid
        if (!IsValidEmail(email))
        {
            return new ServerResponse<RegistrationResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Registration.Error",
                    ResponseDescription = "Invalid email format"
                }
            };
        }

        // Check if the email exists in the database
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Check if the email has already been confirmed/used
            if (existingUser.EmailConfirmed)
            {
                return new ServerResponse<RegistrationResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Registration.Error",
                        ResponseDescription = "Email is already registered and confirmed"
                    }
                };
            }
            else
            {
                return new ServerResponse<RegistrationResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Registration.Error",
                        ResponseDescription = "Email is already registered but not yet confirmed"
                    }
                };
            }
        }

        // Generate a 5-digit verification code
        var verificationCode = GenerateVerificationCode();

        // Send email with verification code
        var emailBody = $"Your verification code is {verificationCode}";
        var emailResult = await _emailService.SendEmailAsync(email, "Email Verification", emailBody);
        if (!emailResult)
        {
            return new ServerResponse<RegistrationResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Registration.Error",
                    ResponseDescription = "Error sending verification email"
                }
            };
        }

        // Store the verification code and email in temporary storage
        await _verificationService.SaveVerificationCode(email, verificationCode);

        // Set the email in EmailContext for future use
        _emailContext.VerifiedEmail = email;

        // Create the response DTO
        var registrationResponse = new RegistrationResponseDto
        {
            Email = email,
            VerificationCode = verificationCode
        };

        return new ServerResponse<RegistrationResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Verification code sent successfully",
            Data = registrationResponse
        };
    }

    public async Task<ServerResponse<EmailVerificationResponseDto>> VerifyEmail(string verificationCode)
    {
        _logger.LogInformation("******* Inside the VerifyEmail Method ********");

        try
        {
            // Retrieve the stored email using the verification code
            var email = await _verificationService.GetEmailByVerificationCode(verificationCode);
            if (email == null)
            {
                return new ServerResponse<EmailVerificationResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Verification.Error",
                        ResponseDescription = "Invalid verification code"
                    }
                };
            }

            // Validate the verification code
            var isCodeValid = await _verificationService.ValidateVerificationCode(email, verificationCode);
            if (!isCodeValid)
            {
                return new ServerResponse<EmailVerificationResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Verification.Error",
                        ResponseDescription = "Invalid verification code"
                    }
                };
            }

            // Mark email as verified
            await _verificationService.MarkEmailAsVerified(email);

            // Check if the email was successfully marked as verified
            var isMarkedVerified = await _verificationService.IsEmailVerified(email);
            if (!isMarkedVerified)
            {
                return new ServerResponse<EmailVerificationResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "500",
                        ResponseMessage = "Verification.Error",
                        ResponseDescription = "Failed to mark email as verified."
                    }
                };
            }

            // Store the verified email in session
            _httpContextAccessor.HttpContext?.Session.SetString("VerifiedEmail", email);

            // Create the response DTO
            var verificationResponse = new EmailVerificationResponseDto
            {
                Email = email,
                IsVerified = true,
                Message = "Email verified. Proceed with registration."
            };

            return new ServerResponse<EmailVerificationResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Email verified successfully",
                Data = verificationResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while verifying email: {ex.Message}");
            return new ServerResponse<EmailVerificationResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Verification.Error",
                    ResponseDescription = "An unexpected error occurred"
                }
            };
        }
    }

    public async Task<ServerResponse<RegisterUserResponseDto>> RegisterSuperAdmin(RegisterSuperAdminDto registerSuperAdminDto)
    {
        _logger.LogInformation("******* Inside the RegisterSuperAdmin Method ********");

        // Fetch the verified email from session storage
        var verifiedEmail = _httpContextAccessor.HttpContext?.Session.GetString("VerifiedEmail");
        if (string.IsNullOrEmpty(verifiedEmail))
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email not verified",
                    ResponseDescription = "The provided email has not been verified."
                }
            };
        }

        if (registerSuperAdminDto.Password != registerSuperAdminDto.ConfirmPassword)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Passwords do not match",
                    ResponseDescription = "Password and confirmation password do not match."
                }
            };
        }

        var names = registerSuperAdminDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = verifiedEmail,
            AppUserName = registerSuperAdminDto.AppUserName,
            Email = verifiedEmail,
            PhoneNumber = registerSuperAdminDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PublicId = Guid.NewGuid().ToString(),
        };

        var superAdmin = new SuperAdmin
        {
            ApplicationUserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerSuperAdminDto.Password);
        if (!result.Succeeded)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "User creation failed",
                    ResponseDescription = string.Join("; ", result.Errors.Select(e => e.Description))
                }
            };
        }

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.SuperAdmin);
        if (!result.Succeeded)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Failed to assign role",
                    ResponseDescription = string.Join("; ", result.Errors.Select(e => e.Description))
                }
            };
        }

        await _superAdminRepository.AddAsync(superAdmin);
        await _verificationService.MarkEmailAsVerified(verifiedEmail);

        // Clear the verified email from the session after registration
        _httpContextAccessor.HttpContext?.Session.Remove("VerifiedEmail");

        return new ServerResponse<RegisterUserResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Registration successful",
            Data = new RegisterUserResponseDto
            {
                UserId = applicationUser.Id,
                AppUserName = applicationUser.AppUserName,
                Email = applicationUser.Email,
                Role = RolesConstant.SuperAdmin,
                Message = "Registration successful."
            }
        };
    }

    public async Task<ServerResponse<RegisterUserResponseDto>> RegisterUser(RegisterUserDto registerUserDto)
    {
        _logger.LogInformation("******* Inside the RegisterUser Method ********");

        // Fetch the verified email directly from the verification service
        var verifiedEmail = _httpContextAccessor.HttpContext?.Session.GetString("VerifiedEmail");
        if (string.IsNullOrEmpty(verifiedEmail))
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email not verified",
                    ResponseDescription = "The provided email has not been verified."
                }
            };
        }

        if (registerUserDto.Password != registerUserDto.ConfirmPassword)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Passwords do not match",
                    ResponseDescription = "Password and confirmation password do not match."
                }
            };
        }

        var names = registerUserDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = verifiedEmail,
            AppUserName = registerUserDto.AppUserName,
            Email = verifiedEmail,
            PhoneNumber = registerUserDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            WalletId = GenerateWalletId(),
            PublicId = Guid.NewGuid().ToString(),
        };

        var regularUser = new RegularUser
        {
            ApplicationUserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerUserDto.Password);
        if (!result.Succeeded)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "User creation failed",
                    ResponseDescription = string.Join("; ", result.Errors.Select(e => e.Description))
                }
            };
        }

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.RegularUser);
        if (!result.Succeeded)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Failed to assign role",
                    ResponseDescription = string.Join("; ", result.Errors.Select(e => e.Description))
                }
            };
        }

        await _regularUserRepository.AddAsync(regularUser);
        // Clear the EmailContext after successful registration
        _httpContextAccessor.HttpContext?.Session.Remove("VerifiedEmail");

        return new ServerResponse<RegisterUserResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Registration successful",
            Data = new RegisterUserResponseDto
            {
                UserId = applicationUser.Id,
                AppUserName = applicationUser.AppUserName,
                Email = applicationUser.Email,
                Role = RolesConstant.RegularUser,
                Message = "Registration successful."
            }
        };
    }

    public async Task<ServerResponse<RegisterUserResponseDto>> RegisterCreator(RegisterCreatorDto registerCreatorDto)
    {
        _logger.LogInformation("******* Inside the RegisterCreator Method ********");

        // Fetch the verified email directly from the verification service
        var verifiedEmail = _httpContextAccessor.HttpContext?.Session.GetString("VerifiedEmail");

        if (string.IsNullOrEmpty(verifiedEmail))
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email not verified",
                    ResponseDescription = "The provided email has not been verified."
                }
            };
        }

        if (registerCreatorDto.Password != registerCreatorDto.ConfirmPassword)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Passwords do not match",
                    ResponseDescription = "Password and confirmation password do not match."
                }
            };
        }

        var names = registerCreatorDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = verifiedEmail,
            AppUserName = registerCreatorDto.AppUserName,
            Email = verifiedEmail,
            PhoneNumber = registerCreatorDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            WalletId = GenerateWalletId(),
            PublicId = Guid.NewGuid().ToString(),
        };

        var creator = new Creator
        {
            ApplicationUserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerCreatorDto.Password);
        if (!result.Succeeded)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "User creation failed",
                    ResponseDescription = string.Join("; ", result.Errors.Select(e => e.Description))
                }
            };
        }

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.Creator);
        if (!result.Succeeded)
        {
            return new ServerResponse<RegisterUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Failed to assign role",
                    ResponseDescription = string.Join("; ", result.Errors.Select(e => e.Description))
                }
            };
        }

        await _creatorRepository.AddAsync(creator);
        await _verificationService.MarkEmailAsVerified(verifiedEmail);
        _httpContextAccessor.HttpContext?.Session.Remove("VerifiedEmail");

        return new ServerResponse<RegisterUserResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Registration successful",
            Data = new RegisterUserResponseDto
            {
                UserId = applicationUser.Id,
                AppUserName = applicationUser.AppUserName,
                Email = applicationUser.Email,
                Role = RolesConstant.Creator,
                Message = "Registration successful."
            }
        };
    }

    public async Task<ServerResponse<LoginResponseDto>> Login(LoginUserDto loginUserDto)
    {
        _logger.LogInformation("******* Inside the Login Method ********");

        var user = await _userManager.FindByEmailAsync(loginUserDto.Email);

        if (user is null)
        {
            _logger.LogWarning("User not found for email: {email}", loginUserDto.Email);
            return new ServerResponse<LoginResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "Your email is incorrect"
                }
            };
        }

        _logger.LogInformation("User found: {email}", user.Email);

        var isValidUser = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);

        if (!isValidUser)
        {
            _logger.LogWarning("Invalid password for email: {email}", user.Email);
            return new ServerResponse<LoginResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "Your password is incorrect"
                }
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Log if user has no roles
        if (roles == null || !roles.Any())
        {
            _logger.LogWarning("User {email} has no roles assigned", user.Email);
        }

        // Generate JWT token and check if the token service works properly
        try
        {
            _logger.LogInformation("Generating token for user {email}", user.Email);
            var token = _jwtService.GenerateToken(user, roles);

            var email = user.Email ?? string.Empty;
            var fullName = user.FullName ?? string.Empty;

            return new ServerResponse<LoginResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Login successful",
                Data = new LoginResponseDto(token, fullName, roles, email)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating token for {email}", user.Email);
            return new ServerResponse<LoginResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "An error occurred during login"
                }
            };
        }
    }


    public async Task<ServerResponse<RegistrationResponseDto>> ForgotPassword(ForgotPasswordRequestDto forgotPasswordDto)
    {
        _logger.LogInformation("******* Inside the ForgotPassword Method ********");

        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

        if (user == null)
        {
            return new ServerResponse<RegistrationResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "No user found with the provided email"
                }
            };
        }

        // Generate a verification code
        var verificationCode = GenerateVerificationCode();

        // Send the verification code via email
        const string emailSubject = "Your Password Reset Verification Code";
        var emailBody = $"Hello {user.FullName}, your verification code is: {verificationCode}.";

        var isSuccessful = await _emailService.SendEmailAsync(forgotPasswordDto.Email, emailSubject, emailBody);
        if (!isSuccessful)
        {
            return new ServerResponse<RegistrationResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "Error occurred while sending the verification email"
                }
            };
        }

        // Store the verification code and email in a temporary storage
        await _verificationService.SaveVerificationCode(forgotPasswordDto.Email, verificationCode);

        // Set the email in EmailContext for future use
        //var email = _httpContextAccessor.HttpContext?.Session.GetString("VerifiedEmail");
        //_emailContext.VerifiedEmail = forgotPasswordDto.Email;
        _httpContextAccessor.HttpContext?.Session.SetString("VerifiedEmail", forgotPasswordDto.Email);


        // Create the response DTO
        var registrationResponse = new RegistrationResponseDto
        {
            Email = forgotPasswordDto.Email,
            VerificationCode = verificationCode

        };

        return new ServerResponse<RegistrationResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Verification code sent successfully",
            Data = registrationResponse
            //Data = new LoginResponseDto(token, fullName, roles, email)

        };
    }

    public async Task<ServerResponse<PasswordResetResponseDto>> VerifyCodeAndResetPassword(ResetPasswordWithCodeDto resetPasswordDto)
    {
        _logger.LogInformation("******* Inside the VerifyCodeAndResetPassword Method ********");

        // Step 1: Retrieve the stored email using the verification code
        var email = await _verificationService.GetEmailByVerificationCode(resetPasswordDto.VerificationCode);
        if (email == null)
        {
            return new ServerResponse<PasswordResetResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Verification.Error",
                    ResponseDescription = "Invalid verification code"
                }
            };
        }

        // Step 2: Validate the verification code (e.g., check expiration and match)
        var isCodeValid = await _verificationService.ValidateVerificationCode(email, resetPasswordDto.VerificationCode);
        if (!isCodeValid)
        {
            return new ServerResponse<PasswordResetResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Verification.Error",
                    ResponseDescription = "Invalid or expired verification code"
                }
            };
        }

        // Step 3: Retrieve user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new ServerResponse<PasswordResetResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "No user found with the provided email"
                }
            };
        }

        // Step 4: Ensure password and confirm password match
        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
        {
            return new ServerResponse<PasswordResetResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "Passwords do not match"
                }
            };
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Step 5: Use the reset token (this should be passed from the frontend along with resetPasswordDto)
        var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDto.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return new ServerResponse<PasswordResetResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Auth.Error",
                    ResponseDescription = "Error occurred while resetting the password"
                }
            };
        }

        // Step 6: Optionally clear the verification code after successful password reset
        await _verificationService.MarkEmailAsVerified(email);

        // Step 7: Return success response
        var passwordResetResponse = new PasswordResetResponseDto
        {
            Email = email,
            IsPasswordReset = true,
            NewPassword = resetPasswordDto.NewPassword,
            Message = "Password reset successful"
        };

        return new ServerResponse<PasswordResetResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Password reset successful",
            Data = passwordResetResponse
        };
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





    public static class EmailContext
    {
        public static string? VerifiedEmail { get; set; }
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

    //public bool IsValidEmail(string email)
    //{
    //    try
    //    {
    //        var addr = new System.Net.Mail.MailAddress(email);
    //        return addr.Address == email;
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}

    private bool IsValidEmail(string email)
    {
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }


}
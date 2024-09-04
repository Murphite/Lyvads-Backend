using Lyvads.API.Presentation.Dtos;
using Lyvads.API.Extensions;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.AuthDtos;
using Microsoft.AspNetCore.Mvc;
using static Lyvads.Application.Implementions.AuthService;
using System.ComponentModel.DataAnnotations;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Implementions;
using Microsoft.AspNetCore.Identity;

namespace Lyvads.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IVerificationService _verificationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IVerificationService verificationService, IAuthService authService, ILogger<AuthController> logger)
    {
        _verificationService = verificationService;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("Initiate")]
    public async Task<IActionResult> InitiateRegistration([FromBody] InitiateRegistrationDto dto)
    {
        _logger.LogInformation("******* Initiate Registration ********");

        var result = await _authService.InitiateRegistration(dto.Email);
        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<RegistrationResponseDto>.Success(result.Data, "Verification code sent. Please check your email."));
    }


    [HttpPost("VerifyEmail")]
    public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationDto dto)
    {
        _logger.LogInformation("******* Inside the VerifyEmail Controller Method ********");

        var result = await _authService.VerifyEmail(dto.VerificationCode);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<EmailVerificationResponseDto>.Success(result.Data, result.Message));
    }

    [HttpPost("RegisterUser")]
    public async Task<IActionResult> RegisterUser([FromBody] CompleteRegistrationDto dto)
    {
        _logger.LogInformation($"******* Inside the RegisterUser Controller Method ********");

        // Retrieve the email from EmailContext
        var email = EmailContext.VerifiedEmail;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email is not set in EmailContext.");
            return BadRequest("Email verification is required.");
        }

        // Retrieve the verified email using the service method
        var verifiedEmail = await _verificationService.GetVerifiedEmail(email);

        if (string.IsNullOrEmpty(verifiedEmail))
        {
            _logger.LogWarning("No verified email found for the provided email.");
            return BadRequest("Email not verified.");
        }

        // Create a RegisterUserDto from the CompleteRegistrationDto
        var registerUserDto = new RegisterUserDto
        {
            FullName = dto.FullName,
            Username = dto.Username,
            PhoneNumber = dto.PhoneNumber,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword,
            Email = verifiedEmail
        };

        var result = await _authService.RegisterUser(registerUserDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<RegisterUserResponseDto>.Success(result.Data, result.Message));
    }

    [HttpPost("RegisterCreator")]
    public async Task<IActionResult> RegisterCreator([FromBody] CompleteRegistrationDto dto)
    {
        _logger.LogInformation($"******* Inside the RegisterUser Controller Method ********");

        // Initialize the EmailHolder and set the email from context or session if needed
        var emailHolder = new EmailHolder
        {
            Email = EmailContext.VerifiedEmail
        };

        if (string.IsNullOrEmpty(emailHolder.Email))
        {
            // Handle the case where the email has not been verified or set
            return BadRequest();
        }

        // Create a RegisterUserDto from the CompleteRegistrationDto
        var registerCreatorDto = new RegisterCreatorDto
        {
            FullName = dto.FullName,
            Username = dto.Username,
            PhoneNumber = dto.PhoneNumber,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword,
            Email = emailHolder.Email
        };

        var result = await _authService.RegisterCreator(registerCreatorDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(result.Message));
    }

    [HttpPost("RegisterAdmin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] CompleteRegistrationDto dto)
    {
        _logger.LogInformation($"******* Inside the RegisterUser Controller Method ********");

        // Initialize the EmailHolder and set the email from context or session if needed
        var emailHolder = new EmailHolder
        {
            Email = EmailContext.VerifiedEmail
        };

        if (string.IsNullOrEmpty(emailHolder.Email))
        {
            // Handle the case where the email has not been verified or set
            return BadRequest(ResponseDto<object>.Failure(new[] { new Error("Email.Error", "Email not verified or set") }));
        }

        // Create a RegisterUserDto from the CompleteRegistrationDto
        var registerAdminDto = new RegisterAdminDto
        {
            FullName = dto.FullName,
            Username = dto.Username,
            PhoneNumber = dto.PhoneNumber,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword,
            Email = emailHolder.Email
        };

        var result = await _authService.RegisterAdmin(registerAdminDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(result.Message));
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
    {
        _logger.LogInformation($"******* Inside the Login Controller Method ********");

        var result = await _authService.Login(loginUserDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(new
        {
            Token = result.Data.Token,
            FullName = result.Data.FullName,
            Roles = result.Data.Roles,
            Email = result.Data.Email    
        }));
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation($"******* Inside the Reset Password Controller Method ********");

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
        }

        var resetPasswordResult = await _authService.ResetPasswordAsync(resetPasswordDto);

        if (resetPasswordResult.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(resetPasswordResult.Errors));

        return Ok(ResponseDto<object>.Success(resetPasswordResult));
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        _logger.LogInformation($"******* Inside the Forgot Password Controller Method ********");

        var result = await _authService.ForgotPassword(resetPasswordDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(result.Message));
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        _logger.LogInformation($"******* Inside the ConfirmEmail Controller Method ********");

        var result = await _authService.ConfirmEmail(email, token);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(result.Message));
    }

    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        _logger.LogInformation($"******* Inside the Change Password Controller Method ********");

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
        }
        var changePasswordResult = await _authService.ChangePasswordAsync(changePasswordDto);
        if (changePasswordResult.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(changePasswordResult.Errors));

        return Ok(ResponseDto<object>.Success(changePasswordResult.Message));
    }

    

    public class EmailHolder
    {
        public string? Email { get; set; }
    }
    public class EmailVerificationDto
    {
        public required string VerificationCode { get; set; }
    }

    public class InitiateRegistrationDto
    {
        public required string Email { get; set; }
    }

    public class CompleteRegistrationDto
    {
        [Required] public required string FullName { get; init; }
        [Required] public required string Username { get; init; }
        [Required] public required string PhoneNumber { get; init; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public required string ConfirmPassword { get; set; }
    }
}
using Lyvads.API.Presentation.Dtos;
using Lyvads.API.Extensions;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.AuthDtos;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("RegisterUser")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto registerUserDto)
    {
        _logger.LogInformation($"******* Inside the RegisterUser Controller Method ********");

        var result = await _authService.RegisterUser(registerUserDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("RegisterCreator")]
    public async Task<IActionResult> RegisterCreator([FromBody] RegisterCreatorDto registerCreatorDto)
    {
        _logger.LogInformation($"******* Inside the RegisterCreator Controller Method ********");

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
        }

        var result = await _authService.RegisterCreator(registerCreatorDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("RegisterAdmin")]
    public async Task<IActionResult> AdminRegister([FromBody] RegisterAdminDto registerAdminDto)
    {
        _logger.LogInformation($"******* Inside the RegisterAdmin Controller Method ********");

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDto<object>.Failure(ModelState.GetErrors()));
        }

        var result = await _authService.RegisterAdmin(registerAdminDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
    {
        _logger.LogInformation($"******* Inside the Login Controller Method ********");

        var result = await _authService.Login(loginUserDto);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(result.Data));
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

        return Ok(ResponseDto<object>.Success());
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        _logger.LogInformation($"******* Inside the ConfirmEmail Controller Method ********");

        var result = await _authService.ConfirmEmail(email, token);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
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

        return Ok(ResponseDto<object>.Success(changePasswordResult));
    }
}
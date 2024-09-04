using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementions;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Lyvads.Application.Implementions.ProfileService;

namespace Lyvads.API.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, UserManager<ApplicationUser> userManager, ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPut("EditProfile")]
    public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _profileService.EditProfileAsync(dto, user.Id);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        // Return the updated profile data
        return Ok(ResponseDto<EditProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    }

    [HttpPut("UpdateProfilePicture")]
    public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto dto)
    {
        // Get the logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Update the profile picture for the logged-in user
        var result = await _profileService.UpdateProfilePictureAsync(user.Id, dto.NewProfilePictureUrl);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(ResponseDto<UpdateProfilePicResponseDto>.Success(result.Data, "Profile picture updated successfully."));
    }

    [HttpPost("InitiateEmailUpdate")]
    public async Task<IActionResult> InitiateEmailUpdate([FromBody] UpdateEmailDto dto)
    {
        _logger.LogInformation("******* Initiating Email Update ********");

        if (string.IsNullOrEmpty(dto.UserId))
        {
            return BadRequest("User ID cannot be null or empty");
        }

        if (string.IsNullOrEmpty(dto.NewEmail))
        {
            return BadRequest("New Email cannot be null or empty");
        }

        var result = await _profileService.InitiateEmailUpdateAsync(dto.UserId, dto.NewEmail);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<UpdateEmailResponseDto>.Success(result.Data, "Verification code sent to the new email. Please check your email."));
    }

    [HttpPost("VerifyEmailUpdate")]
    public async Task<IActionResult> VerifyEmailUpdate([FromBody] EmailUpdateVerificationDto dto)
    {
        _logger.LogInformation("******* Verifying Email Update ********");

        if (string.IsNullOrEmpty(dto.UserId))
        {
            return BadRequest("User ID cannot be null or empty");
        }

        if (string.IsNullOrEmpty(dto.VerificationCode))
        {
            return BadRequest("Verification Code cannot be null or empty");
        }

        var result = await _profileService.VerifyEmailUpdateAsync(dto.UserId, dto.VerificationCode);

        if (result.IsFailure)
        {
            return BadRequest(ResponseDto<object>.Failure(result.Errors));
        }

        return Ok(ResponseDto<EmailVerificationResponseDto>.Success(result.Data, result.Message));
    }


    [HttpPut("UpdateLocation")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the user's location
        var result = await _profileService.UpdateLocationAsync(dto, user.Id);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        // Return the updated location data
        return Ok(ResponseDto<UpdateLocationResponseDto>.Success(result.Data, "Location updated successfully."));
    }

    [HttpPut("UpdatePhoneNumber")]
    public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the user's phone number
        var result = await _profileService.UpdatePhoneNumberAsync(dto, user.Id);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        // Return the updated phone number data
        return Ok(ResponseDto<UpdatePhoneNumberResponseDto>.Success(result.Data, "Phone number updated successfully."));
    }



    public class EmailUpdateVerificationDto
    {
        public string? UserId { get; set; } 
        public string? VerificationCode { get; set; } 
    }



    public class UpdateEmailDto
    {
        public string? UserId { get; set; }
        public string? NewEmail { get; set; }  
    }

}

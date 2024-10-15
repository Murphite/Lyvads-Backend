using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Lyvads.Application.Implementations.ProfileService;

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

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        // Return the updated profile data
        return Ok(result);
    }

    [HttpPut("UpdateProfilePicture")]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto dto)
    {
        // Get the logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Validate the file (photo) if provided
        if (dto.NewProfilePictureUrl != null && !IsValidFile(dto.NewProfilePictureUrl))  // Validate using the Image property
        {
            return BadRequest(new { message = "Invalid File Extension" });
        }

        // Convert IFormFile to byte array
        byte[] fileBytes = null!;
        if (dto.NewProfilePictureUrl != null)
        {
            using (var stream = new MemoryStream())
            {
                await dto.NewProfilePictureUrl.CopyToAsync(stream);  // Use the Image property for file operations
                fileBytes = stream.ToArray();
            }
        }

        // Update the profile picture for the logged-in user (no need to pass folderName)
        var result = await _profileService.UpdateProfilePictureAsync(user.Id, dto.NewProfilePictureUrl!);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    private bool IsValidFile(IFormFile file)
    {
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        return validExtensions.Contains(extension);
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

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
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

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);


        return Ok(result);
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

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        // Return the updated location data
        return Ok(result);
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

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        // Return the updated phone number data
        return Ok(result);
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


    //public class UpdateProfilePictureDto
    //{
    //    public IFormFile NewProfilePicture { get; set; }
    //}

}

using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
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
    private readonly IUserRepository _userRepository;
    private readonly IUserInteractionService _userInteractionService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, UserManager<ApplicationUser> userManager, 
        ILogger<ProfileController> logger, IUserInteractionService userInteractionService, 
        IUserRepository userRepository)
    {
        _profileService = profileService;
        _userManager = userManager;
        _logger = logger;
        _userInteractionService = userInteractionService;
        _userRepository = userRepository;
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

    [HttpGet("GetCreatorsProfile")]
    public async Task<IActionResult> ViewCreatorProfile()
    {
        // Get the logged-in user's ID
        var userId = _userManager.GetUserId(User);

        // Retrieve the user with the Creator entity included
        var user = await _userRepository.GetUserWithCreatorAsync(userId);
        if (user == null)
            return NotFound("User not found.");

        if (user.Creator == null)
            return BadRequest("User is not a creator.");

        var response = await _userInteractionService.ViewCreatorProfileAsync(user.Creator.Id);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
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

        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        if (string.IsNullOrEmpty(dto.NewEmail))
            return BadRequest("New Email cannot be null or empty");

        var result = await _profileService.InitiateEmailUpdateAsync(user.Id, dto.NewEmail);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPost("VerifyEmailUpdate")]
    public async Task<IActionResult> VerifyEmailUpdate([FromBody] EmailUpdateVerificationDto dto)
    {
        _logger.LogInformation("******* Verifying Email Update ********");

        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        if (string.IsNullOrEmpty(dto.VerificationCode))
        {
            return BadRequest("Verification Code cannot be null or empty");
        }

        var result = await _profileService.VerifyEmailUpdateAsync(user.Id, dto.VerificationCode);

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

    
    [HttpGet("GetProfile")]
    public async Task<IActionResult> GetProfile()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to get the user profile
        var result = await _profileService.GetProfileAsync(user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        // Return the profile data
        return Ok(result.Data);
    }

    [HttpPost("ValidatePassword")]
    public async Task<IActionResult> ValidatePassword([FromBody] ValidatePasswordDto validatePasswordDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        _logger.LogInformation($"******* Inside the ValidatePassword Controller Method ********");

        var result = await _profileService.ValidatePasswordAsync(user.Email!, validatePasswordDto);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }









  
    public class EmailUpdateVerificationDto
    {
        public string? VerificationCode { get; set; } 
    }



    public class UpdateEmailDto
    {
        public string? NewEmail { get; set; }  
    }


    //public class UpdateProfilePictureDto
    //{
    //    public IFormFile NewProfilePicture { get; set; }
    //}

}

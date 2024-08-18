using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementions;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RegularUserController : Controller
{
    private readonly IRegularUserService _regularUserService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegularUserController> _logger;

    public RegularUserController(IRegularUserService regularUserService, UserManager<ApplicationUser> userManager, ILogger<RegularUserController> logger)
    {
        _regularUserService = regularUserService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPut("UpdateProfilePicture")]
    public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto dto)
    {
        if (string.IsNullOrEmpty(dto.NewProfilePictureUrl))
            return BadRequest("Invalid input data.");

        /// Get the logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Assuming you have a service that handles both regular users and creators
        var result = await _regularUserService.UpdateProfilePictureAsync(user.Id, dto.NewProfilePictureUrl);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(ResponseDto<UpdateProfilePicResponseDto>.Success(result.Data, "Profile picture updated successfully."));
    }


    [HttpPut("EditProfile")]
    public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _regularUserService.EditProfileAsync(dto, user.Id);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        // Return the updated profile data
        return Ok(ResponseDto<EditProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    }

    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateRegularUserProfileDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _regularUserService.UpdateUserProfileAsync(dto, user.Id);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        // Return the updated profile data
        return Ok(ResponseDto<RegularUserProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    }
}

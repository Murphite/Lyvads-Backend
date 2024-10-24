using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
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


    //[HttpPut("UpdateProfile")]
    //public async Task<IActionResult> UpdateProfile([FromBody] UpdateRegularUserProfileDto dto)
    //{
    //    // Get the logged-in user's ID
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return NotFound("User not found.");

    //    // Call the service to update the creator profile
    //    var result = await _regularUserService.UpdateUserProfileAsync(dto, user.Id);

    //    if (!result.IsSuccess)
    //        return BadRequest(result.Errors);

    //    // Return the updated profile data
    //    return Ok(ResponseDto<RegularUserProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    //}

    
    [HttpGet("RegularUsers")]
    public async Task<IActionResult> GetRegularUsers([FromQuery] PaginationFilter paginationFilter)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _regularUserService.GetRegularUsers(paginationFilter);

        if (!result.IsSuccessful)
            return StatusCode(int.Parse(result.ResponseCode), result.ErrorResponse);

        return Ok(result);
    }
}

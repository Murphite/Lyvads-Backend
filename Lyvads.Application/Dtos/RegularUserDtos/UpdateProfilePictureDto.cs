
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class UpdateProfilePictureDto
{
    public IFormFile? NewProfilePictureUrl { get; set; }
}


public class UpdateProfilePicResponseDto
{
    public string UserId { get; set; } = default!;
    public string NewProfilePictureUrl { get; set; } = default!;
}

public class UpdateEmailResponseDto
{
    public string Email { get; set; } = default!;
    public string? VerificationCode { get; set; }
}

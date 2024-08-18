
namespace Lyvads.Application.Dtos.RegularUserDtos;

public class UpdateProfilePictureDto
{
    public string NewProfilePictureUrl { get; set; } = default!;
}

public class UpdateProfilePicResponseDto
{
    public string UserId { get; set; } = default!;
    public string NewProfilePictureUrl { get; set; } = default!;
}

public class UpdateEmailResponseDto
{
    public string Email { get; set; } = default!;
    public string VerificationCode { get; set; }
}

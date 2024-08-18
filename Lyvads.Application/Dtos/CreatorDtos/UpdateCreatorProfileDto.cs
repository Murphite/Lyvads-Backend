

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class UpdateCreatorProfileDto
{
    public string? SimpleAdvert { get; set; }
    public string? WearBrand { get; set; }
    public string? SongAdvert { get; set; }
    public RequestType? Request { get; set; }
}

public class UpdateRegularUserProfileDto
{
    public string? FullName { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Location { get; set; }
    public string? Occupation { get; set; }
    public string? Bio { get; set; }
    public string? VerificationCode { get; set; }

}


public class EditProfileDto
{
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public string? Bio { get; set; }
}
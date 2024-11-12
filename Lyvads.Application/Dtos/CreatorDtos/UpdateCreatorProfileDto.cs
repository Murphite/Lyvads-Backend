

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class UpdateCreatorProfileDto
{
    public string? SimpleAdvert { get; set; }
    public string? WearBrand { get; set; }
    public string? SongAdvert { get; set; }
    public string? Request { get; set; }
}

public class RateDto
{
    public string RateId { get; set; } = string.Empty; // Only for response
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateCreatorRateDto
{
    public List<RateDto> Rates { get; set; } = new List<RateDto>();

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

public class UserProfileDto
{
    public string FullName { get; set; } = default!;
    public string Bio { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string AppUsername { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string ProfilePic { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Location { get; set; } = default!;
}

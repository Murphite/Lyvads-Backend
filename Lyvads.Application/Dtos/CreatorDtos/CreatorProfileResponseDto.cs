﻿

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class CreatorProfileResponseDto
{
    public string? SimpleAdvert { get; set; }
    public string? WearBrand { get; set; }
    public string? SongAdvert { get; set; }
    public string? Request { get; set; }
}

public class CreatorRateResponseDto
{
    public List<RateDto> Rates { get; set; } = new List<RateDto>();
    public string? CreatorId { get; set; }
}

public class RegularUserProfileResponseDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Location { get; set; }
    public string? Occupation { get; set; }
    public string? Bio { get; set; }
}

public class EditProfileResponseDto
{
    public string? FullName { get; set; }
    public string? AppUsername { get; set; }
    public string? Bio { get; set; }

}

public class ValidatePasswordDto
{
    public string? Password { get; set; }
}

public class PostEditDto
{
    //public string? Caption { get; set; }
    //public string? Location { get; set; }
    //public PostVisibility? Visibility { get; set; }
    public List<string>? MediaToDelete { get; set; }
}    


public class DeleteMediaRequestDto
{
    public string? PostId { get; set; }
    public List<string>? MediaToDelete { get; set; }
}

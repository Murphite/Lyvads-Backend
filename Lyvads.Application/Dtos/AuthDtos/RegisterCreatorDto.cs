using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterCreatorDto
{
    public required string? FullName { get; init; }
    public required string? AppUserName { get; init; }
    public required string? PhoneNumber { get; init; }
    public required string? Email { get; init; }
    public required string? Bio { get; init; }
    public required string? Location { get; init; }
    public required string? Occupation { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public required string? Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string? ConfirmPassword { get; set; }
    public SocialHandlesDto? SocialHandles { get; set; } 
    public List<ExclusiveDealDto>? ExclusiveDeals { get; set; }  
}

public class SocialHandlesDto
{
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? XTwitter { get; set; }
    public string? Tiktok { get; set; }
}

public class ExclusiveDealDto
{
    public string? Industry { get; set; }
    public string? BrandName { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterCreatorDto
{
    public required string FullName { get; init; }
    public required string AppUserName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string? ConfirmPassword { get; set; }
}

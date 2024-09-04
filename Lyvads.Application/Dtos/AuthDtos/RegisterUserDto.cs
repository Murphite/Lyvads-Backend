using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterUserDto
{
    [Required] public required string FullName { get; init; }
    [Required] public required string Username { get; init; }
    [Required] public required string PhoneNumber { get; init; }
    [EmailAddress] public required string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string? ConfirmPassword { get; set; }
}
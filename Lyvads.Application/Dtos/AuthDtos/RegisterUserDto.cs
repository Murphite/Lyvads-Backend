using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterUserDto
{
    [Required] public string FullName { get; init; }
    [Required] public string Username { get; init; }
    [Required] public string PhoneNumber { get; init; }
    [EmailAddress] public string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string ConfirmPassword { get; set; }
}
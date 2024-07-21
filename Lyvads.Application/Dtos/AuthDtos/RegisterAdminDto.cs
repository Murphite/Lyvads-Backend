using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterAdminDto
{
    [Required] public string FullName { get; init; } = default!;
    [Required] public string PhoneNumber { get; init; } = default!;
    [Required] public string Username { get; init; } = default!;
    [Required][EmailAddress] public string Email { get; init; } = default!;

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string ConfirmPassword { get; set; } = default!;
}
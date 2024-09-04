using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class ChangePasswordDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string OldPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Password and confirm password do not match")]
    public required string ConfirmPassword { get; set; }
}
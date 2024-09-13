using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterAdminDto
{
    public required string FullName { get; init; } = default!;
    public required string PhoneNumber { get; init; } = default!;
    //public required string AppUserName { get; init; } = default!;
    [EmailAddress] public required string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string ConfirmPassword { get; set; } = default!;
}

public class RegisterSuperAdminDto
{
    public required string FullName { get; init; } = default!;
    public required string AppUserName { get; init; } = default!;
    public required string PhoneNumber { get; init; } = default!;
    [EmailAddress] public required string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string ConfirmPassword { get; set; } = default!;
}
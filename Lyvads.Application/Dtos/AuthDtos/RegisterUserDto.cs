using Lyvads.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterUserDto
{
    [Required] public required string FullName { get; init; }
    [Required] public required string AppUserName { get; init; }
    [Required] public required string PhoneNumber { get; init; }
    [Required] public required string Location { get; init; }
    [EmailAddress] public required string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string? ConfirmPassword { get; set; }
    public string? Role { get; set; }
}

public class AdminRegisterUserDto
{
    [Required] public required string FirstName { get; init; }
    [Required] public required string LastName { get; init; }
    [Required] public required string PhoneNumber { get; init; }
    [Required] public required string Location { get; init; }
    [EmailAddress] public required string Email { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string? ConfirmPassword { get; set; }
    public string? Role { get; set; }
}

public enum UserRoleEnum
{
    Admin = 1,
    SuperAdmin = 2
}

public class AdminRegisterUsersDto
{
    [Required] public required string FirstName { get; init; }
    [Required] public required string LastName { get; init; }
    [Required] public required string PhoneNumber { get; init; }
    [Required] public required string Location { get; init; }
    [EmailAddress] public required string Email { get; init; }
    public string? Role { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public class ResetPasswordDto
{
    [EmailAddress] public required string Email { get; set; }

    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }

    public required string Token { get; set; }
}
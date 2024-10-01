using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public record LoginUserDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

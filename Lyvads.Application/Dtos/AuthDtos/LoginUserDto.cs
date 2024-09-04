using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos.AuthDtos;

public record LoginUserDto
{
    [Required] public required string Email { get; set; }

    [Required] public required string Password { get; set; }
}



using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos;

public class WaitlistDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}

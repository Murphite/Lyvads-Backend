

using System.ComponentModel.DataAnnotations;

namespace Lyvads.Application.Dtos;

public class WaitlistDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

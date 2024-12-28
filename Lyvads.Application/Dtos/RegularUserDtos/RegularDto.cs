

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class RegularUserDto
{
    public string? UserId { get; set; } 
    public string? FullName { get; set; } 
    public string? Email { get; set; } 
    public string? ProfilePictureUrl { get; set; } 
    public DateTimeOffset CreatedAt { get; set; } 
    public string? AppUserName { get; set; } 
}

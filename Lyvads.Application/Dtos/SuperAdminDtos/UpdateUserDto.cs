

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class UpdateUserDto
{
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? email { get; set; }
    public string? phoneNumber { get; set; }
    public string? location { get; set; }
}

public class UserDto
{
    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string? ProfilePic { get; set; }
    public string? Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
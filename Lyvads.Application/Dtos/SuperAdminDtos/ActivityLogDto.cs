

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class ActivityLogDto
{
    public string? UserId { get; set; }
    public string? UserPic { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? Role { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }

}
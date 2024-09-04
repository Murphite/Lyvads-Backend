
namespace Lyvads.Application.Dtos.CreatorDtos;

public class PostResponseDto
{
    public string? PostId { get; set; }
    public string? CreatorId { get; set; }
    public string? CreatorName { get; set; } 
    public string? Caption { get; set; }
    public string? MediaUrl { get; set; }
    public string? Location { get; set; }
    public string? Visibility { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

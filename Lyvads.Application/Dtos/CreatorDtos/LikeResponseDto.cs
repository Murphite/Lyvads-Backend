

namespace Lyvads.Application.Dtos.CreatorDtos;

public class LikeResponseDto
{
    public string? LikeId { get; set; }
    public string? CommentId { get; set; }
    public string? UserId { get; set; }
    public string? LikedBy { get; set; } 
    public string? PostId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
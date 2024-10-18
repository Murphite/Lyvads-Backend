

namespace Lyvads.Domain.Entities;

public class Like : Entity
{
    public string? UserId { get; set; } = default!;
    public string? CommentId { get; set; } = default!;
    public string? PostId { get; set; } = default!;
    public string? ContentId { get; set; } = default!;
    public string? LikedBy { get; set; } = default!;

    public Post? Post { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public Content? Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

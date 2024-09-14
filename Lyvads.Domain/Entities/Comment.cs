

namespace Lyvads.Domain.Entities;

public class Comment : Entity, IAuditable
{
    public string? PostId { get; set; }
    public string? UserId { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? CommentBy { get; set; }
    public bool IsDeleted { get; set; } 
    public Post? Post { get; set; }

    public ApplicationUser User { get; set; }

    // For replies
    public string? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; } 
    public ICollection<Comment>? Replies { get; set; } 
}

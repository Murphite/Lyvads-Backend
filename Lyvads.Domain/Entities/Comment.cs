
namespace Lyvads.Domain.Entities;

public class Comment : Entity, IAuditable
{
    public string? PostId { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? CommentBy { get; set; }
    public bool IsDeleted { get; set; }
    public Post? Post { get; set; }

    // For replies
    public string? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment>? Replies { get; set; }


    public string? RegularUserId { get; set; }
    public RegularUser? RegularUser { get; set; }
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    
}

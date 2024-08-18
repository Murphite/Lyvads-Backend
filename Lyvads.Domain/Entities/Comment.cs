

namespace Lyvads.Domain.Entities;

public class Comment : Entity, IAuditable
{
    public string PostId { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CommentBy { get; set; }
    public Post Post { get; set; }
    
}



namespace Lyvads.Domain.Entities;

public class Like : Entity
{
    public string UserId { get; set; }
    public string CommentId { get; set; }
    public string PostId { get; set; }
    public string ContentId { get; set; }

    public Post Post { get; set; }
    public RegularUser User { get; set; }
    public Content Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

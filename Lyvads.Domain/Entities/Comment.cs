

namespace Lyvads.Domain.Entities;

public class Comment : Entity
{
    public string PostId { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public RegularUser User { get; set; }
    public Post Post { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;

public class Post : Entity, IAuditable
{
    public string Caption { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public bool IsDeleted { get; set; }

    public Creator Creator { get; set; } = default!;
    public PostVisibility Visibility { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public PostStatus PostStatus { get; set; }
}



using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Post : Entity
{
    public string Caption { get; set; } = default!;
    public string MediaUrl { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string CreatorId { get; set; } = default!;

    public Creator Creator { get; set; } = default!;
    public PostVisibility Visibility { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public PostStatus PostStatus { get; set; } 
}

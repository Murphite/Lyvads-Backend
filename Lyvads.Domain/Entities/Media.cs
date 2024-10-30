
namespace Lyvads.Domain.Entities;

public class Media : Entity, IAuditable
{
    public string? PostId { get; set; }
    public string? Url { get; set; }
    public string? FileType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Post? Post { get; set; } 
}

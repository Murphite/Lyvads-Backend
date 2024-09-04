
namespace Lyvads.Domain.Entities;

public class RegularUser : Entity, IAuditable
{
    public string? UserId { get; set; }
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }
}

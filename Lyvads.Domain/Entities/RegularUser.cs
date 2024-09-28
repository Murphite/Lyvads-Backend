
namespace Lyvads.Domain.Entities;

public class RegularUser : ApplicationUser, IAuditable
{
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? ApplicationUserId { get; set; } = default!;
    public ApplicationUser? ApplicationUser { get; set; }
}

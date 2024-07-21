

namespace Lyvads.Domain.Entities;

public class RegularUser : ApplicationUser
{
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

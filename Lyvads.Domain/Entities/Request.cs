using Lyvads.Domain.Enums;

namespace Lyvads.Domain.Entities;

public class Request : Entity
{
    public string Type { get; set; } = default!;
    public string Script { get; set; } = default!;
    public string? CreatorId { get; set; }
    public Creator Creator { get; set; } = default!;
    public string? UserId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public RegularUser User { get; set; } = default!;
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}    

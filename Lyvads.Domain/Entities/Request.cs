using Lyvads.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Request : Entity, IAuditable
{
    public string Type { get; set; } = default!;
    public string Script { get; set; } = default!;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal FastTrackFee { get; set; }
    public string? CreatorId { get; set; }
    public Creator Creator { get; set; } = default!;
    public PaymentMethod PaymentMethod { get; set; }
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public string? VideoUrl { get; set; }

    public string? RegularUserId { get; set; }
    public RegularUser? RegularUser { get; set; }


}

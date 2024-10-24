using Lyvads.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Request : Entity, IAuditable
{
    [Required]
    public string Script { get; set; } = default!;
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive number.")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal FastTrackFee { get; set; } = 0m;
    
    public AppPaymentMethod PaymentMethod { get; set; }
    public RequestType RequestType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? VideoUrl { get; set; }
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    

    public string? RegularUserId { get; set; }
    public RegularUser? RegularUser { get; set; }
    public string? CreatorId { get; set; }
    public Creator Creator { get; set; } = default!;


}

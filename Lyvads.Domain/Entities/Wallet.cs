


using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Wallet : Entity, IAuditable
{
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Balance { get; set; } = 0;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();

    // Foreign key to ApplicationUser
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}



namespace Lyvads.Domain.Entities;

public class Wallet : Entity
{
    public decimal Balance { get; set; } = 0;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();


    // Foreign key to ApplicationUser
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
}

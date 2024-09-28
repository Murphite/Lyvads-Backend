using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Promotion
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ShortDescription { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsHidden { get; set; } = false; // Default is to show the promotion
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

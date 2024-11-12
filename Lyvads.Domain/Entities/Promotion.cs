using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Promotion : Entity
{
    public string? Title { get; set; }
    public string? ShortDescription { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsHidden { get; set; } = false; 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

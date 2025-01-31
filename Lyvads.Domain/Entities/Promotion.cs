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


public class PromotionPlan : Entity
{
    public string Name { get; set; } = string.Empty; // e.g., "Monthly Plan"
    public string Description { get; set; } = string.Empty;    
    public int Price { get; set; }
    public int DurationInDays { get; set; } // 30 for Monthly, 365 for Annual
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PromotionSubscription : Entity
{
    public string CreatorId { get; set; } = string.Empty;
    public string PromotionPlanId { get; set; } = string.Empty; // Ensure it's not nullable
    public string PaymentReference { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime SubscriptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    // Navigation Properties
    public Creator? Creator { get; set; }
    public PromotionPlan? PromotionPlan { get; set; }

    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser? ApplicationUser { get; set; }
}

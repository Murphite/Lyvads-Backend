using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class CreatePromotionDto
{
    [Required]
    public required string Title { get; set; }
    public required string ShortDescription { get; set; }
    public decimal Price { get; set; }
    public IFormFile? Media { get; set; } // Media can be a photo or video file uploaded
}

public class UpdatePromotionDto
{
    public string? Title { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public IFormFile? Media { get; set; } // Optional media update
}

public class PromotionDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? ShortDescription { get; set; }
    public decimal? Price { get; set; }
    public string? MediaUrl { get; set; }
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePromotionPlanDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public int DurationInDays { get; set; } // e.g., 30 for monthly, 365 for annually
}

public class SubscribedCreatorDto
{
    public string? SubscriptionId { get; set; }
    public string? CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string CreatorImage { get; set; } = string.Empty;
    public string CreatorOccupation { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int? AmountPaid { get; set; }
    public DateTime SubscriptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}

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


﻿

using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Creator : Entity, IAuditable
{
    public ICollection<Content> Contents { get; set; } = new List<Content>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    // Social Media Handles
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? XTwitter { get; set; }
    public string? Tiktok { get; set; }


    // Creator SetUp Rates
    public List<Rate> Rates { get; set; } = new List<Rate>();   
    //public ICollection<CollaborationRate>? CollabRates { get; set; }


    // Exclusive Deal Properties
    public bool HasExclusiveDeal { get; set; }
    public ICollection<ExclusiveDeal> ExclusiveDeals { get; set; } = new List<ExclusiveDeal>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AdvertAmount { get; set; }


    // Navigation Property   
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser? ApplicationUser { get; set; }

    [NotMapped]
    public int EngagementCount => Posts.Sum(p => p.Likes.Count + p.Comments.Count);

}

public class ExclusiveDeal : Entity
{
    public string Industry { get; set; } = default!;
    public string BrandName { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public Creator Creator { get; set; } = default!;
}



public class Rate : Entity, IAuditable
{
    public string? Type { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }
    public string? CreatorId { get; set; }
    public Creator? Creator { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}


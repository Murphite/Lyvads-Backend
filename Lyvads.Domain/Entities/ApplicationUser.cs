﻿using Lyvads.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class ApplicationUser : IdentityUser, IAuditable
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public new string? PhoneNumber { get; set; }
    public string? AppUserName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Occupation { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? PublicId { get; set; } = default!;
    public bool IsVerified { get; set; }
    public string? FullName => $"{FirstName} {LastName}";
    public string? VerificationCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Wallet Navigation Property
    public Wallet Wallet { get; set; } = default!;

    public ICollection<Notification>? Notifications { get; set; } = new List<Notification>();
    public ICollection<Like>? Likes { get; set; } = new List<Like>();
    public string? StripeAccountId { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    //public int Followers { get; set; }
    public AdminPermission AdminPermissions { get; set; } = default!;
    public RegularUser? RegularUser { get; set; }
    public Creator? Creator { get; set; }
    public Admin? Admin { get; set; }
    public SuperAdmin? SuperAdmin { get; set; }
}

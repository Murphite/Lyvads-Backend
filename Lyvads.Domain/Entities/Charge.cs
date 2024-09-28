

using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Charge : Entity, IAuditable
{
    public ChargeReason ChargeName { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Percentage { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal MinAmount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal MaxAmount { get; set; } 
    public ChargeStatus Status { get; set; } 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class ChargeTransaction : Entity, IAuditable
{
    public string UserName { get; set; } = default!;
    public ChargeReason ChargeName { get; set; } 
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public CTransStatus Status { get; set; } 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ApplicationUserId {  get; set; } = default!;  
    public ApplicationUser ApplicationUser { get; set; } = default!;
}
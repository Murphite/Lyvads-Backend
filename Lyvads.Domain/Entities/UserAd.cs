using Lyvads.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Domain.Entities;

public class UserAd : Entity, IAuditable
{
    public string UserName { get; set; } = default!;
    public string Description { get; set; } = default!;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public UserAdStatus Status { get; set; } 

    public string? ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

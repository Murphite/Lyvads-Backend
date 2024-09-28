using Lyvads.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Domain.Entities;

public class UserAd : Entity, IAuditable
{
    public string UserName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Amount { get; set; }
    public UserAdStatus Status { get; set; } 
    public int UserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

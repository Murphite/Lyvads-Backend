

using Lyvads.Domain.Constants;

namespace Lyvads.Domain.Entities;

public class ActivityLog : Entity
{
    public string UserName { get; set; } // Name of the user who performed the activity
    public DateTime Date { get; set; } // Date and time of the activity
    public RolesConstant Role { get; set; }
    public string Description { get; set; } // Description of the activity
    public string Category { get; set; } // Category of the activity (Promo, General, Users, etc.)
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
}


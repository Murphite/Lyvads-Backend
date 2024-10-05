
namespace Lyvads.Domain.Entities;

public class AdminPermission : Entity
{

    public bool CanManageAdminRoles { get; set; }
    public bool CanManageUsers { get; set; }
    public bool CanManageRevenue { get; set; }
    public bool CanManageUserAds { get; set; }
    public bool CanManageCollaborations { get; set; }
    public bool CanManagePosts { get; set; }
    public bool CanManageDisputes { get; set; }
    public bool CanManagePromotions { get; set; }

    public string ApplicationUserId { get; set; } = default!;
    public virtual ApplicationUser ApplicationUser { get; set; } = default!;
    public string? RoleId { get; set; }  
    public virtual AdminRole? AdminRole { get; set; }
}


public class AdminRole : Entity
{
    public string? RoleName { get; set; }
}

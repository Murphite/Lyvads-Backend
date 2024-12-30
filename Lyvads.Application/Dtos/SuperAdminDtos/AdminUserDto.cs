

using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;


public class CreateCustomRoleRequestDto
{
    public string? RoleName { get; set; }
    public AdminPermissionsDto? Permissions { get; set; }
}


public class AdminUserDto
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; } 
    public bool IsActive { get; set; }
    public DateTimeOffset LastActive { get; set; }
}


//public class AdminRole
//{
//    public string?? Id { get; set; }
//    public string?? RoleName { get; set; } // "Admin" or "SuperAdmin"
//    public string?? AdminUserId { get; set; } // Reference to the admin user
//    public ApplicationUser AdminUser { get; set; }
//}


public class AdminPermissionsDto
{
    public bool CanManageAdminRoles { get; set; }
    public bool CanManageUsers { get; set; }
    public bool CanManageRevenue { get; set; }
    public bool CanManageUserAds { get; set; }
    public bool CanManageCollaborations { get; set; }
    public bool CanManagePosts { get; set; }
    public bool CanManageDisputes { get; set; }
    public bool CanManagePromotions { get; set; }
}


public class AddAdminUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; } 
}


public class AddAdminUserResponseDto
{
    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}


public class EditAdminUserDto
{
    //public string? Id { get; set; } // Admin user ID
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public AdminRoleType Role { get; set; }
    public bool IsActive { get; set; }
}

public class EditResponseAdminUserDto
{
    public string? Id { get; set; } // Admin user ID
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
}




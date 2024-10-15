

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Lyvads.Application.Implementations;

public class AdminPermissionsService : IAdminPermissionsService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<AdminPermissionsService> _logger;
    private readonly IAdminRepository _adminRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminPermissionsService(IActivityLogRepository activityLogRepository,
        ILogger<AdminPermissionsService> logger,
        IAdminRepository adminRepository,
        UserManager<ApplicationUser> userManager)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
        _adminRepository = adminRepository;
        _userManager = userManager;
    }


    public async Task<ServerResponse<List<AdminUserDto>>> GetAllAdminUsersAsync()
    {
        var admins = await _adminRepository.GetAllAdminsAsync(); // Fetch all Admin and SuperAdmin

        if (admins == null || !admins.Any())
        {
            return new ServerResponse<List<AdminUserDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No admin users found.",
                Data = null
            };
        }

        var adminUserDtos = admins.Select(a => new AdminUserDto
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            Role = a is SuperAdmin ? AdminRoleType.SuperAdmin : AdminRoleType.Admin,
            LastActive = a.UpdatedAt,
            IsActive = a.IsActive
        }).ToList();

        return new ServerResponse<List<AdminUserDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Admin users fetched successfully.",
            Data = adminUserDtos
        };
    }

    public async Task<ServerResponse<AdminPermissionsDto>> GrantPermissionsToAdminAsync(string adminUserId,
        AdminPermissionsDto permissionsDto, string requestingAdminId)
    {
        // Check if the requesting user is a SuperAdmin
        var requestingAdmin = await _userManager.FindByIdAsync(requestingAdminId);
        if (requestingAdmin == null || !(await _userManager.IsInRoleAsync(requestingAdmin, AdminRoleType.SuperAdmin.ToString())))
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Only SuperAdmins can grant permissions.",
                Data = null
            };
        }

        // Find the admin user who will receive the permissions
        var adminUser = await _userManager.FindByIdAsync(adminUserId);
        if (adminUser == null)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Admin user not found.",
                Data = null
            };
        }

        if (!adminUser.IsActive)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Cannot assign permissions to an inactive user.",
                Data = null
            };
        }

        // Check if the adminUser is actually an Admin or SuperAdmin
        var isAdminOrSuperAdmin = await _userManager.IsInRoleAsync(adminUser, AdminRoleType.Admin.ToString())
                                    || await _userManager.IsInRoleAsync(adminUser, AdminRoleType.SuperAdmin.ToString());

        if (!isAdminOrSuperAdmin)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "User is not an Admin or SuperAdmin.",
                Data = null
            };
        }

        // Logic to update permissions for the admin user
        var permissions = new AdminPermission
        {
            ApplicationUserId = adminUserId,
            CanManageAdminRoles = permissionsDto.CanManageAdminRoles,
            CanManageUsers = permissionsDto.CanManageUsers,
            CanManageRevenue = permissionsDto.CanManageRevenue,
            CanManageUserAds = permissionsDto.CanManageUserAds,
            CanManageCollaborations = permissionsDto.CanManageCollaborations,
            CanManagePosts = permissionsDto.CanManagePosts,
            CanManageDisputes = permissionsDto.CanManageDisputes,
            CanManagePromotions = permissionsDto.CanManagePromotions
        };

        // Save the permissions to the database
        await _adminRepository.AddAsync(permissions);

        // Return the granted permissions in the response
        return new ServerResponse<AdminPermissionsDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Permissions granted successfully.",
            Data = permissionsDto
        };
    }

    public async Task<ServerResponse<string>> CreateCustomRoleAsync(string roleName, AdminPermissionsDto permissionsDto)
    {
        // Validate if the role name is within the allowed roles
        var validRoles = new[] { RolesConstant.Admin, RolesConstant.SuperAdmin }; // Add more valid roles if needed

        if (!validRoles.Contains(roleName))
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid role name.",
                Data = null
            };
        }

        // Check if the role already exists in the repository
        var existingRole = await _adminRepository.GetRoleByNameAsync(roleName);
        if (existingRole != null)
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Role already exists.",
                Data = null
            };
        }

        // Create a new AdminRole entity
        var newRole = new AdminRole
        {
            RoleName = roleName
        };

        // Save the role to the repository
        await _adminRepository.AddRoleAsync(newRole);

        // Create the new permissions for the role and associate them
        var newPermissions = new AdminPermission
        {
            AdminRoleId = newRole.Id,
            CanManageAdminRoles = permissionsDto.CanManageAdminRoles,
            CanManageUsers = permissionsDto.CanManageUsers,
            CanManageRevenue = permissionsDto.CanManageRevenue,
            CanManageUserAds = permissionsDto.CanManageUserAds,
            CanManageCollaborations = permissionsDto.CanManageCollaborations,
            CanManagePosts = permissionsDto.CanManagePosts,
            CanManageDisputes = permissionsDto.CanManageDisputes,
            CanManagePromotions = permissionsDto.CanManagePromotions
        };

        // Save the permissions to the repository
        await _adminRepository.AddAsync(newPermissions);

        // Return success response
        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Custom role created successfully with associated permissions.",
            Data = roleName
        };
    }


    public async Task<EditAdminUserDto> EditAdminUserAsync(EditAdminUserDto editAdminUserDto)
    {
        var adminUser = await _userManager.FindByIdAsync(editAdminUserDto.Id);
        if (adminUser == null)
        {
            return null; // Return null if user is not found, or handle the error appropriately
        }

        // Update basic user information
        adminUser.FirstName = editAdminUserDto.FirstName;
        adminUser.LastName = editAdminUserDto.LastName;

        // Get current roles of the admin user
        var currentRoles = await _userManager.GetRolesAsync(adminUser);

        // Remove the user from all current roles
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(adminUser, currentRoles);
        }

        // Add the user to the new role based on the provided AdminRoleType
        await _userManager.AddToRoleAsync(adminUser, editAdminUserDto.Role.ToString());

        // Update the IsActive status
        adminUser.IsActive = editAdminUserDto.IsActive;

        // Save the changes to the user
        await _userManager.UpdateAsync(adminUser);

        // Return the updated DTO
        return new EditAdminUserDto
        {
            Id = adminUser.Id,
            FirstName = adminUser.FirstName,
            LastName = adminUser.LastName,
            Role = editAdminUserDto.Role,
            IsActive = adminUser.IsActive
        };
    }

    public async Task<AddAdminUserDto> AddAdminUserAsync(AddAdminUserDto addAdminUserDto)
    {
        var newAdminUser = new ApplicationUser
        {
            FirstName = addAdminUserDto.FirstName,
            LastName = addAdminUserDto.LastName,
            Email = addAdminUserDto.Email,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var result = await _userManager.CreateAsync(newAdminUser, addAdminUserDto.Password);
        if (!result.Succeeded)
        {
            return null;
        }

        // Assign the initial role (Admin or SuperAdmin) after creation
        await _userManager.AddToRoleAsync(newAdminUser, addAdminUserDto.Role.ToString());

        // Return the created DTO
        return new AddAdminUserDto
        {
            FirstName = newAdminUser.FirstName,
            LastName = newAdminUser.LastName,
            Email = newAdminUser.Email,
            Role = addAdminUserDto.Role
        };
    }


}

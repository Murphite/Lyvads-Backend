

using CloudinaryDotNet.Actions;
using Lyvads.Application.Dtos.AuthDtos;
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
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly IRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminPermissionsService(IActivityLogRepository activityLogRepository,
        ILogger<AdminPermissionsService> logger,
        IAdminRepository adminRepository,
        ISuperAdminRepository superAdminRepository,
        IRepository repository,
        UserManager<ApplicationUser> userManager)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
        _adminRepository = adminRepository;
        _superAdminRepository = superAdminRepository;
        _repository = repository;
        _userManager = userManager;
    }


    public async Task<ServerResponse<List<AdminUserDto>>> GetAllAdminUsersAsync()
    {
        var admins = await _adminRepository.GetAllAdminsAsync();

        if (admins == null || !admins.Any())
        {
            return new ServerResponse<List<AdminUserDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No admin users found.",
                Data = null!
            };
        }

        var adminUserDtos = new List<AdminUserDto>();

        foreach (var a in admins)
        {
            // Assuming you're fetching the role from Identity or some other method
            var roles = await _userManager.GetRolesAsync(a);  // or from the repository
            var role = roles.Contains("SuperAdmin") ? AdminRoleType.SuperAdmin : AdminRoleType.Admin;

            adminUserDtos.Add(new AdminUserDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                Role = role.ToString(),
                LastActive = a.UpdatedAt,
                IsActive = a.IsActive
            });
        }

        return new ServerResponse<List<AdminUserDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Admin users fetched successfully.",
            Data = adminUserDtos
        };
    }


    public async Task<ServerResponse<AdminPermissionsDto>> GrantPermissionsToAdminAsync(string superAdminUserId,
     AdminPermissionsDto permissionsDto, string targetAdminId)
    {
        
        var targetAdmin = await _userManager.FindByIdAsync(targetAdminId);
        var superAdmin = await _userManager.FindByIdAsync(superAdminUserId);

        // Check for the target admin user
        if (targetAdmin == null)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Target admin user not found.",
                Data = null!
            };
        }

        // Check if the requesting admin is a SuperAdmin

        //if (!(await _userManager.GetRolesAsync(superAdmin)
        var roles = await _userManager.GetRolesAsync(superAdmin!);
        if (roles == null || !roles.Contains("SuperAdmin"))
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Only SuperAdmins can grant permissions.",
                Data = null!
            };
        }

        // Find the admin user who will receive the permissions
       // var adminUser = await _userManager.FindByIdAsync(targetAdmin);
        if (targetAdmin == null)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Admin user not found.",
                Data = null!
            };
        }

        if (!targetAdmin.IsActive)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Cannot assign permissions to an inactive user.",
                Data = null!
            };
        }

        // Check if the adminUser is actually an Admin or SuperAdmin
        var isAdminOrSuperAdmin = await _userManager.IsInRoleAsync(targetAdmin, AdminRoleType.Admin.ToString()) ||
                                  await _userManager.IsInRoleAsync(targetAdmin, AdminRoleType.SuperAdmin.ToString());

        if (!isAdminOrSuperAdmin)
        {
            return new ServerResponse<AdminPermissionsDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "User is not an Admin or SuperAdmin.",
                Data = null!
            };
        }

        // Logic to update or create permissions for the admin user
        var existingPermissions = await _adminRepository.GetByUserIdAsync(superAdminUserId); 

        if (existingPermissions != null)
        {
            // Update the existing permissions
            existingPermissions.CanManageAdminRoles = permissionsDto.CanManageAdminRoles;
            existingPermissions.CanManageUsers = permissionsDto.CanManageUsers;
            existingPermissions.CanManageRevenue = permissionsDto.CanManageRevenue;
            existingPermissions.CanManageUserAds = permissionsDto.CanManageUserAds;
            existingPermissions.CanManageCollaborations = permissionsDto.CanManageCollaborations;
            existingPermissions.CanManagePosts = permissionsDto.CanManagePosts;
            existingPermissions.CanManageDisputes = permissionsDto.CanManageDisputes;
            existingPermissions.CanManagePromotions = permissionsDto.CanManagePromotions;

            await _adminRepository.UpdateAsync(existingPermissions);  
        }
        else
        {
            // If no existing permissions, create new ones
            var permissions = new AdminPermission
            {
                ApplicationUserId = targetAdminId,
                CanManageAdminRoles = permissionsDto.CanManageAdminRoles,
                CanManageUsers = permissionsDto.CanManageUsers,
                CanManageRevenue = permissionsDto.CanManageRevenue,
                CanManageUserAds = permissionsDto.CanManageUserAds,
                CanManageCollaborations = permissionsDto.CanManageCollaborations,
                CanManagePosts = permissionsDto.CanManagePosts,
                CanManageDisputes = permissionsDto.CanManageDisputes,
                CanManagePromotions = permissionsDto.CanManagePromotions
            };

            await _adminRepository.AddAsync(permissions);  
        }

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
        var role = roleName?.ToUpperInvariant();      

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


    public async Task<ServerResponse<EditResponseAdminUserDto>> EditAdminUserAsync(string adminUserId, EditAdminUserDto editAdminUserDto)
    {
        if (string.IsNullOrEmpty(adminUserId))
        {
            return new ServerResponse<EditResponseAdminUserDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Admin User ID cannot be null or empty.",
                Data = null!
            };
        }

        var adminUser = await _adminRepository.GetAdminByIdAsync(adminUserId); 
        if (adminUser == null)
        {
            return new ServerResponse<EditResponseAdminUserDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Admin user not found.",
                Data = null!
            };
        }

        adminUser.FirstName = editAdminUserDto.FirstName!;
        adminUser.LastName = editAdminUserDto.LastName!;

        var currentRoles = await _userManager.GetRolesAsync(adminUser);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(adminUser, currentRoles);
        }

        await _userManager.AddToRoleAsync(adminUser, editAdminUserDto.Role.ToString());
        adminUser.IsActive = editAdminUserDto.IsActive;
        await _userManager.UpdateAsync(adminUser);

        return new ServerResponse<EditResponseAdminUserDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Admin user updated successfully.",
            Data = new EditResponseAdminUserDto
            {
                Id = adminUserId,
                FirstName = adminUser.FirstName,
                LastName = adminUser.LastName,
                Role = editAdminUserDto.Role.ToString(),
                IsActive = adminUser.IsActive
            }
        };
    }


    public async Task<ServerResponse<AddAdminUserDto>> AddAdminUserAsync(AddAdminUserDto addAdminUserDto)
    {
        using (var transaction = await _repository.BeginTransactionAsync())
        {

            var newAdminUser = new ApplicationUser
            {
                FirstName = addAdminUserDto.FirstName!,
                LastName = addAdminUserDto.LastName!,
                Email = addAdminUserDto.Email,
                UserName = addAdminUserDto.Email,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                PublicId = Guid.NewGuid().ToString(),
                IsActive = true,
            };

            var result = await _userManager.CreateAsync(newAdminUser, addAdminUserDto.Password!);
            if (!result.Succeeded)
            {
                return new ServerResponse<AddAdminUserDto>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Failed to create user.",
                    Data = null
                };
            }

            result = await _userManager.AddToRoleAsync(newAdminUser, addAdminUserDto.Role!.ToString());
            if (!result.Succeeded)
            {
                _logger.LogError("Error occurred while assigning role to user {UserEmail}", addAdminUserDto.Email);
                return new ServerResponse<AddAdminUserDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "500",
                        ResponseMessage = "Role Assignment Failed",
                        ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
                    }
                };
            }

            var role = addAdminUserDto.Role?.ToUpperInvariant();


            // Add the user to the corresponding role repository
            switch (role)
            {
                case RolesConstant.Admin:
                    await _adminRepository.AddAsync(new Admin
                    {
                        ApplicationUserId = newAdminUser.Id,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        ApplicationUser = newAdminUser
                    });
                    break;

                case RolesConstant.SuperAdmin:
                    await _superAdminRepository.AddAsync(new SuperAdmin
                    {
                        ApplicationUserId = newAdminUser.Id,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        ApplicationUser = newAdminUser
                    });
                    break;
            }
                    await transaction.CommitAsync();

            return new ServerResponse<AddAdminUserDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Admin user created successfully.",
                Data = new AddAdminUserDto
                {
                    UserId = newAdminUser.Id,
                    FirstName = newAdminUser.FirstName,
                    LastName = newAdminUser.LastName,
                    Email = newAdminUser.Email,
                    Password = addAdminUserDto.Password,
                    Role = role
                }
            };
        }
    }


}


//// Validate if the role name is within the allowed roles
//var validRoles = new[] { RolesConstant.Admin, RolesConstant.SuperAdmin }; 

//if (!validRoles.Contains(role))
//{
//    return new ServerResponse<string>
//    {
//        IsSuccessful = false,
//        ResponseCode = "400",
//        ResponseMessage = "Invalid role name.",
//        Data = null!
//    };
//}

// Check if the role already exists in the repository
//var existingRole = await _adminRepository.GetRoleByNameAsync(roleName);
//if (existingRole != null)
//{
//    return new ServerResponse<string>
//    {
//        IsSuccessful = false,
//        ResponseCode = "400",
//        ResponseMessage = "Role already exists.",
//        Data = null!
//    };
//}
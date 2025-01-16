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
using System.Security.Cryptography;

namespace Lyvads.Application.Implementations;

public class AdminPermissionsService : IAdminPermissionsService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<AdminPermissionsService> _logger;
    private readonly IAdminRepository _adminRepository;
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly IRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;


    public AdminPermissionsService(IActivityLogRepository activityLogRepository,
        ILogger<AdminPermissionsService> logger,
        IAdminRepository adminRepository,
        ISuperAdminRepository superAdminRepository,
        IRepository repository,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
        _adminRepository = adminRepository;
        _superAdminRepository = superAdminRepository;
        _repository = repository;
        _userManager = userManager;
        _currentUserService = currentUserService;
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
            // Fetch roles for the admin
            var roles = await _userManager.GetRolesAsync(a);
            _logger.LogInformation($"User {a.Id} has roles: {string.Join(", ", roles)}");

            // Determine role based on retrieved roles
            AdminRoleType role = roles.Contains("SuperAdmin")
                ? AdminRoleType.SuperAdmin
                : AdminRoleType.Admin; // Assume only "SuperAdmin" and "Admin" are valid

            adminUserDtos.Add(new AdminUserDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                Role = role.ToString(), // Use the determined role
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
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
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
        var existingPermissions = await _adminRepository.GetByUserIdAsync(targetAdminId); 

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


    public async Task<ServerResponse<RoleWithPermissionsDto>> CreateCustomRoleAsync(string roleName, AdminPermissionsDto permissionsDto)
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

        // Prepare the response object with Permissions as a List
        var response = new RoleWithPermissionsDto
        {
            Id = newRole.Id,
            RoleName = newRole.RoleName,
            Permissions = new List<PermissionsDto>  // Wrap the single PermissionsDto in a list
        {
            new PermissionsDto
            {
                CanManageAdminRoles = newPermissions.CanManageAdminRoles,
                CanManageUsers = newPermissions.CanManageUsers,
                CanManageRevenue = newPermissions.CanManageRevenue,
                CanManageUserAds = newPermissions.CanManageUserAds,
                CanManageCollaborations = newPermissions.CanManageCollaborations,
                CanManagePosts = newPermissions.CanManagePosts,
                CanManageDisputes = newPermissions.CanManageDisputes,
                CanManagePromotions = newPermissions.CanManagePromotions
            }
        }
        };

        // Return success response
        return new ServerResponse<RoleWithPermissionsDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Custom role created successfully with associated permissions.",
            Data = response
        };
    }


    public async Task<ServerResponse<List<RoleWithPermissionsDto>>> GetAllRolesWithPermissionsAsync()
    {
        var roles = await _adminRepository.GetAllRolesWithPermissionsAsync();

        if (roles == null || !roles.Any())
        {
            return new ServerResponse<List<RoleWithPermissionsDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No roles found.",
                Data = null
            };
        }

        var roleWithPermissions = roles.Select(role => new RoleWithPermissionsDto
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Permissions = role.AdminPermissions.Select(permission => new PermissionsDto
            {
                CanManageAdminRoles = permission.CanManageAdminRoles,
                CanManageUsers = permission.CanManageUsers,
                CanManageRevenue = permission.CanManageRevenue,
                CanManageUserAds = permission.CanManageUserAds,
                CanManageCollaborations = permission.CanManageCollaborations,
                CanManagePosts = permission.CanManagePosts,
                CanManageDisputes = permission.CanManageDisputes,
                CanManagePromotions = permission.CanManagePromotions
            }).ToList()  
        }).ToList();

        return new ServerResponse<List<RoleWithPermissionsDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Roles and permissions retrieved successfully.",
            Data = roleWithPermissions
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
        var user = await _userManager.FindByIdAsync(adminUserId);

        //var adminUser = await _adminRepository.GetAdminByIdAsync(adminUserId); 
        // Check if the user is in either Admin or SuperAdmin roles
        var isAdminOrSuperAdmin = await _userManager.IsInRoleAsync(user, RolesConstant.Admin) ||
                                  await _userManager.IsInRoleAsync(user, RolesConstant.SuperAdmin);
        if (!isAdminOrSuperAdmin)
        {
            return new ServerResponse<EditResponseAdminUserDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Admin user not found.",
                Data = null!
            };
        }

        user.FirstName = editAdminUserDto.FirstName ?? user.FirstName;
        user.LastName = editAdminUserDto.LastName ?? user.LastName;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, editAdminUserDto.Role.ToString());
        user.IsActive = editAdminUserDto.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new ServerResponse<EditResponseAdminUserDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Failed to update Admin user.",
                Data = null!
            };
        }

        return new ServerResponse<EditResponseAdminUserDto>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Admin user updated successfully.",
            Data = new EditResponseAdminUserDto
            {
                Id = adminUserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = editAdminUserDto.Role.ToString(),
                IsActive = user.IsActive
            }
        };
    }


    public async Task<ServerResponse<AddAdminUserResponseDto>> AddAdminUserAsync(AddAdminUserDto addAdminUserDto)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (currentUser == null)
        {
            _logger.LogWarning("Current user not found: {UserId}", currentUserId);
            return new ServerResponse<AddAdminUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found",
                    ResponseDescription = "The current user does not exist."
                }
            };
        }

        var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, RolesConstant.SuperAdmin);
        if (!isSuperAdmin)
        {
            _logger.LogWarning("Unauthorized user registration attempt by user: {UserId}", currentUserId);
            return new ServerResponse<AddAdminUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "Unauthorized",
                    ResponseDescription = "Only SuperAdmin can register users."
                }
            };
        }

        // Validate the role
        var validRoles = new[] { RolesConstant.Admin, RolesConstant.SuperAdmin };
        if (string.IsNullOrWhiteSpace(addAdminUserDto.Role) || !validRoles.Contains(addAdminUserDto.Role.ToUpper()))
        {
            return new ServerResponse<AddAdminUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Invalid Role",
                    ResponseDescription = "The selected role is invalid."
                }
            };
        }

        var role = addAdminUserDto.Role?.ToUpperInvariant();

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

        // Generate a random password
        var generatedPassword = GenerateRandomPassword();

        using (var transaction = await _repository.BeginTransactionAsync())
        {
            var result = await _userManager.CreateAsync(newAdminUser, generatedPassword!);
            if (!result.Succeeded)
            {
                return new ServerResponse<AddAdminUserResponseDto>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Failed to create user.",
                    Data = null
                };
            }

            result = await _userManager.AddToRoleAsync(newAdminUser, role);
            if (!result.Succeeded)
            {
                _logger.LogError("Error occurred while assigning role to user {UserEmail}", addAdminUserDto.Email);
                return new ServerResponse<AddAdminUserResponseDto>
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

            return new ServerResponse<AddAdminUserResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Admin user created successfully.",
                Data = new AddAdminUserResponseDto
                {
                    UserId = newAdminUser.Id,
                    FirstName = newAdminUser.FirstName,
                    LastName = newAdminUser.LastName,
                    Email = newAdminUser.Email,
                    Password = generatedPassword,
                    Role = role
                }
            };
        }
    }


    public async Task<ServerResponse<string>> DeleteAdminUserAsync(string userId)
    {
        _logger.LogInformation("Attempting to delete Admin user with ID: {UserId}", userId);

        // Fetch the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} does not exist.", userId);
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found",
                    ResponseDescription = "The specified user does not exist."
                }
            };
        }

        // Check if the user is in either Admin or SuperAdmin roles
        var isAdminOrSuperAdmin = await _userManager.IsInRoleAsync(user, RolesConstant.Admin) ||
                                  await _userManager.IsInRoleAsync(user, RolesConstant.SuperAdmin);

        if (!isAdminOrSuperAdmin)
        {
            _logger.LogWarning("User with ID {UserId} is not an Admin or SuperAdmin.", userId);
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Unauthorized",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "Unauthorized",
                    ResponseDescription = "The user is not an Admin or SuperAdmin."
                }
            };
        }

        try
        {
            // Delete related entities for Admins or SuperAdmins
            if (await _userManager.IsInRoleAsync(user, RolesConstant.Admin))
            {
                await _adminRepository.DeleteRelatedEntitiesAsync(user);
            }
            else if (await _userManager.IsInRoleAsync(user, RolesConstant.SuperAdmin))
            {
                await _adminRepository.DeleteRelatedEntitiesAsync(user);
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description);
                _logger.LogError("Failed to delete Admin user {UserId}. Errors: {Errors}", userId, string.Join(", ", errors));

                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Failed to delete Admin user",
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Deletion failed",
                        ResponseDescription = string.Join(", ", errors)
                    }
                };
            }

            _logger.LogInformation("Admin user with ID {UserId} deleted successfully.", userId);
            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "200",
                ResponseMessage = "Admin user deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting Admin user with ID: {UserId}", userId);
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while deleting the Admin user",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    private string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*";
        const string digits = "0123456789";
        var password = new char[length];

        // Step 1: Ensure there's at least one digit
        using (var rng = RandomNumberGenerator.Create()) // Create an instance of RandomNumberGenerator
        {
            password[0] = digits[RandomNumberGenerator.GetInt32(digits.Length)]; // Use the static method on the class itself

            // Step 2: Fill the rest of the password with random characters from validChars
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes); // Use the instance method GetBytes

            for (int i = 1; i < length; i++)  // Start from index 1 to avoid overwriting the first digit
            {
                password[i] = validChars[randomBytes[i] % validChars.Length];
            }
        }

        // Step 3: Shuffle the password to ensure the digit is randomly placed
        var random = new Random();
        password = password.OrderBy(c => random.Next()).ToArray();  // Shuffle using Random

        return new string(password);
    }

}

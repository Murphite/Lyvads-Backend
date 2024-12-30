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


    public async Task<ServerResponse<string>> DeleteAdminUser(string userId)
    {
        _logger.LogInformation($"Attempting to delete Admin user with ID: {userId}");

        // Fetch the user with related entities
        var user = await _adminRepository.GetUserWithRelatedEntitiesAsync(userId);

        // Ensure the user exists and is an Admin
        if (user == null || user.Admin == null || user.SuperAdmin == null)
        {
            _logger.LogWarning($"User with ID {userId} is not an Admin or does not exist.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Admin or SuperAdmin user not found"
            };
        }

        try
        {
            // Delete related entities
            await _adminRepository.DeleteRelatedEntitiesAsync(user);

            // Attempt to delete the user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description);
                _logger.LogError($"Failed to delete Admin user {userId}. Errors: {string.Join(", ", errors)}");

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

            _logger.LogInformation($"Admin user with ID {userId} deleted successfully.");
            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "200",
                ResponseMessage = "Admin user deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting Admin user with ID: {userId}");
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

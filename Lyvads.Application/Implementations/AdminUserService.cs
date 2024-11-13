

using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Implementations;

public class AdminUserService : ISuperAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminRepository _adminRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminDashboardService> _logger;
    private readonly IRegularUserRepository _regularUserRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly IRepository _repository;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        IAdminRepository adminRepository,
        ICurrentUserService currentUserService,
        ILogger<AdminDashboardService> logger,
        ISuperAdminRepository superAdminRepository,
        ICreatorRepository creatorRepository,
        IRegularUserRepository regularUserRepository,
        IRepository repository)
    {
        _userManager = userManager;
        _adminRepository = adminRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _regularUserRepository = regularUserRepository;
        _creatorRepository = creatorRepository;
        _superAdminRepository = superAdminRepository;
        _repository = repository;
    }

    public async Task<ServerResponse<List<UserDto>>> GetUsers(string? role = null, bool sortByDate = true)
    {
        try
        {
            var users = _userManager.Users.AsQueryable();

            // Filter by role if a role is provided
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                users = users.Where(u => usersInRole.Contains(u));
            }

            // Sort by CreatedAt date if required
            users = sortByDate ? users.OrderBy(u => u.CreatedAt) : users;

            // Execute the query and materialize to list before processing async calls
            var userList = await users.ToListAsync();

            // Map users to UserDto with async calls outside LINQ
            var userDtos = new List<UserDto>();
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault();

                userDtos.Add(new UserDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Location = user.Location,
                    ProfilePic = user.ImageUrl,
                    Role = roleName,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                });
            }

            _logger.LogInformation("Successfully retrieved user list");
            return new ServerResponse<List<UserDto>> { IsSuccessful = true, Data = userDtos };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users");
            return new ServerResponse<List<UserDto>>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    public async Task<ServerResponse<AddUserResponseDto>> AddUser(AdminRegisterUserDto registerUserDto)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (currentUser == null)
        {
            _logger.LogWarning("Current user not found: {UserId}", currentUserId);
            return new ServerResponse<AddUserResponseDto>
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
            return new ServerResponse<AddUserResponseDto>
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

        if (registerUserDto.Password != registerUserDto.ConfirmPassword)
        {
            return new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Password Mismatch",
                    ResponseDescription = "Passwords do not match."
                }
            };
        }

        // Validate the role
        var validRoles = new[] { RolesConstant.Creator, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.SuperAdmin };

        if (string.IsNullOrWhiteSpace(registerUserDto.Role) || !validRoles.Contains(registerUserDto.Role.ToUpper()))
        {
            return new ServerResponse<AddUserResponseDto>
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

        var role = registerUserDto.Role?.ToUpperInvariant();

        var applicationUser = new ApplicationUser
        {
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            UserName = registerUserDto.Email,
            Location = registerUserDto.Location,
            Email = registerUserDto.Email,
            PhoneNumber = registerUserDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            PublicId = Guid.NewGuid().ToString(),
        };
               

        // Start a transaction
        using (var transaction = await _repository.BeginTransactionAsync())
        {
            try
            {
                // Create the user
                var result = await _userManager.CreateAsync(applicationUser, registerUserDto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("Error occurred while creating user {UserEmail}", registerUserDto.Email);
                    return new ServerResponse<AddUserResponseDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "500",
                            ResponseMessage = "User Creation Failed",
                            ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
                        }
                    };
                }

                // Add user to role
                result = await _userManager.AddToRoleAsync(applicationUser, role);
                if (!result.Succeeded)
                {
                    _logger.LogError("Error occurred while assigning role to user {UserEmail}", registerUserDto.Email);
                    return new ServerResponse<AddUserResponseDto>
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
                            ApplicationUserId = applicationUser.Id,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            ApplicationUser = applicationUser
                        });
                        break;

                    case RolesConstant.SuperAdmin:
                        await _superAdminRepository.AddAsync(new SuperAdmin
                        {
                            ApplicationUserId = applicationUser.Id,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            ApplicationUser = applicationUser
                        });
                        break;

                    case RolesConstant.Creator:
                        var creator = new Creator
                        {
                            ApplicationUserId = applicationUser.Id,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            ApplicationUser = applicationUser,
                            Instagram = null,
                            Facebook = null,
                            XTwitter = null,
                            Tiktok = null,
                            // ExclusiveDeals = registerCreatorDto.ExclusiveDeals?.Select(deal => new ExclusiveDeal
                            // {
                            //     Industry = deal.Industry!,
                            //     BrandName = deal.BrandName!,
                            //     CreatorId = applicationUser.Id
                            // }).ToList() ?? new List<ExclusiveDeal>() // Default to empty list if null
                        };
                        await _creatorRepository.AddAsync(creator);
                        break;

                    case RolesConstant.RegularUser:
                        var regularUser = new RegularUser
                        {
                            ApplicationUserId = applicationUser.Id,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            ApplicationUser = applicationUser,
                        };
                        await _regularUserRepository.AddAsync(regularUser);
                        break;
                }

                // Commit transaction if everything is successful
                await transaction.CommitAsync();

                var addUserResponse = new AddUserResponseDto
                {
                    FullName = applicationUser.FullName,
                    UserId = applicationUser.Id,
                    Email = applicationUser.Email,
                    ProfilePictureUrl = applicationUser.ImageUrl,
                    Location = applicationUser.Location,
                    Role = role,
                    Message = $"{applicationUser.Email} registration successful."
                };

                _logger.LogInformation("User {c} registered successfully as {UserRole}", applicationUser.Email, role);
                return new ServerResponse<AddUserResponseDto> { IsSuccessful = true, Data = addUserResponse };
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                _logger.LogError("Error occurred while processing user registration: {ErrorMessage}", ex.Message);
                return new ServerResponse<AddUserResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "500",
                        ResponseMessage = "Registration Failed",
                        ResponseDescription = "An error occurred during user registration."
                    }
                };
            }
        }
    }


    public async Task<ServerResponse<string>> UpdateUser(UpdateUserDto updateUserDto, string userId)
    {
        _logger.LogInformation($"Attempting to update user with ID: {userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found"
            };
        }

        user.FirstName = updateUserDto.firstName ?? user.FirstName;
        user.LastName = updateUserDto.lastName ?? user.LastName;
        user.Email = updateUserDto.email ?? user.Email;
        user.PhoneNumber = updateUserDto.phoneNumber ?? user.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => new ErrorResponse
            {
                ResponseCode = error.Code,
                ResponseMessage = error.Description
            }).ToList();

            _logger.LogError($"Failed to update user {userId}. Errors: {string.Join(", ", errors.Select(e => e.ResponseMessage))}");

            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to update user",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Update failed",
                    ResponseDescription = string.Join(", ", errors.Select(e => e.ResponseMessage))
                }
            };
        }

        _logger.LogInformation($"User with ID {userId} updated successfully.");
        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "User updated successfully"
        };
    }

    public async Task<ServerResponse<string>> DeleteUser(string userId)
    {
        _logger.LogInformation($"Attempting to delete user with ID: {userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found"
            };
        }

        // Soft delete by setting IsActive to false
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => new ErrorResponse
            {
                ResponseCode = error.Code,
                ResponseMessage = error.Description
            }).ToList();

            _logger.LogError($"Failed to delete user {userId}. Errors: {string.Join(", ", errors.Select(e => e.ResponseMessage))}");

            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to delete user",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Deletion failed",
                    ResponseDescription = string.Join(", ", errors.Select(e => e.ResponseMessage))
                }
            };
        }

        _logger.LogInformation($"User with ID {userId} deleted successfully.");
        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "User deleted successfully"
        };
    }


    public async Task<ServerResponse<string>> DisableUser(string userId)
    {
        _logger.LogInformation($"Attempting to disable user with ID: {userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found"
            };
        }

        user.LockoutEnabled = true;
        user.IsActive = false;
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);  // Effectively disables user

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => new ErrorResponse
            {
                ResponseCode = error.Code,
                ResponseMessage = error.Description
            }).ToList();

            _logger.LogError($"Failed to disable user {userId}. Errors: {string.Join(", ", errors.Select(e => e.ResponseMessage))}");

            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to disable user",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Disable failed",
                    ResponseDescription = string.Join(", ", errors.Select(e => e.ResponseMessage))
                }
            };
        }

        _logger.LogInformation($"User with ID {userId} disabled successfully.");
        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "User disabled successfully"
        };
    }

    public async Task<ServerResponse<string>> ActivateUserAsync(string userId)
    {
        // Retrieve the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found",
                    ResponseDescription = "The user with the provided ID does not exist."
                }
            };
        }

        // Check if the user is already active
        if (user.IsActive)
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "User is already active",
                    ResponseDescription = "The user is already active, no further action is required."
                }
            };
        }

        // Check if the user is a Creator
        var isCreator = await _userManager.IsInRoleAsync(user, RolesConstant.Creator);

        // If the user is a Creator and not verified, verify them
        if (isCreator && !user.IsVerified)
        {
            user.IsVerified = true;
        }
        else if (!isCreator && !user.IsVerified)
        {
            // If it's a regular user, you can decide whether you want to auto-verify them or not
            user.IsVerified = true;
        }

        // Activate the user
        user.IsActive = true;

        // Update the user in the database
        await _userManager.UpdateAsync(user);

        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseMessage = "User has been activated and verified (if necessary) successfully.",
            Data = "User activation complete"
        };
    }


    public async Task<ServerResponse<string>> ToggleUserStatusAsync(string userId)
    {
        _logger.LogInformation($"Attempting to toggle user status for ID: {userId}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found"
            };
        }

        // Toggle user's status
        if (user.IsActive)
        {
            // Disable the user
            user.LockoutEnabled = true;
            user.IsActive = false;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);  // Effectively disables user
            _logger.LogInformation($"User with ID {userId} disabled.");
        }
        else
        {
            // Enable the user
            user.IsActive = true;
            user.LockoutEnd = null;  // Removes any lockout, re-enabling the account

            var isCreator = await _userManager.IsInRoleAsync(user, RolesConstant.Creator);
            if (isCreator && !user.IsVerified)
            {
                user.IsVerified = true;  // Verify Creators if necessary
            }
            else if (!isCreator && !user.IsVerified)
            {
                user.IsVerified = true;  // Optionally verify regular users
            }

            _logger.LogInformation($"User with ID {userId} activated.");
        }

        // Update the user in the database
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => new ErrorResponse
            {
                ResponseCode = error.Code,
                ResponseMessage = error.Description
            }).ToList();

            _logger.LogError($"Failed to toggle user status for {userId}. Errors: {string.Join(", ", errors.Select(e => e.ResponseMessage))}");

            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to toggle user status",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Toggle failed",
                    ResponseDescription = string.Join(", ", errors.Select(e => e.ResponseMessage))
                }
            };
        }

        string message = user.IsActive ? "User activated successfully" : "User disabled successfully";
        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = message,
            Data = message
        };
    }


}
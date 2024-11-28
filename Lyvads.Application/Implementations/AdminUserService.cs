

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
using Microsoft.AspNetCore.Http;
using Lyvads.Domain.Interfaces;
using System.Security.Cryptography;

namespace Lyvads.Application.Implementations;

public class AdminUserService : ISuperAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminRepository _adminRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IProfileService _profileService;
    private readonly ILogger<AdminDashboardService> _logger;
    private readonly IRegularUserRepository _regularUserRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly ISuperAdminRepository _superAdminRepository;
    private readonly IRepository _repository;
    private readonly IWalletRepository _walletRepository;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        IAdminRepository adminRepository,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        IProfileService profileService,
        ILogger<AdminDashboardService> logger,
        ISuperAdminRepository superAdminRepository,
        ICreatorRepository creatorRepository,
        IRegularUserRepository regularUserRepository,
        IRepository repository,
        IWalletRepository walletRepository)
    {
        _userManager = userManager;
        _adminRepository = adminRepository;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _profileService = profileService;
        _logger = logger;
        _regularUserRepository = regularUserRepository;
        _creatorRepository = creatorRepository;
        _superAdminRepository = superAdminRepository;
        _repository = repository;
        _walletRepository = walletRepository;
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

    public async Task<ServerResponse<AddUserResponseDto>> AddUser(AdminRegisterUsersDto registerUserDto)
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

        // Generate a random password
        var generatedPassword = GenerateRandomPassword();

        using (var transaction = await _repository.BeginTransactionAsync())
        {
            try
            {
                // Create the user with the generated password
                var result = await _userManager.CreateAsync(applicationUser, generatedPassword);
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

                // Add user to corresponding role repository
                await AddUserToRoleRepository(role, applicationUser);

                var wallet = new Wallet
                {
                    ApplicationUserId = applicationUser.Id,
                    Balance = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                // Save the wallet to the database
                await _walletRepository.AddAsync(wallet);

                // Commit transaction
                await transaction.CommitAsync();

                // Send the password via email
                var emailBody = $"Your verification code is {generatedPassword}";
                var emailResult = await _emailService.SendEmailAsync(applicationUser.Email, "Lyvads Account Password", emailBody);

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

                _logger.LogInformation("User {Email} registered successfully as {Role}", applicationUser.Email, role);
                return new ServerResponse<AddUserResponseDto> { IsSuccessful = true, Data = addUserResponse };
            }
            catch (Exception ex)
            {
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



    private async Task AddUserToRoleRepository(string role, ApplicationUser user)
    {
        switch (role)
        {
            case RolesConstant.Admin:
                await _adminRepository.AddAsync(new Admin { ApplicationUserId = user.Id, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, ApplicationUser = user });
                break;
            case RolesConstant.SuperAdmin:
                await _superAdminRepository.AddAsync(new SuperAdmin { ApplicationUserId = user.Id, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, ApplicationUser = user });
                break;
            case RolesConstant.Creator:
                await _creatorRepository.AddAsync(new Creator { ApplicationUserId = user.Id, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, ApplicationUser = user });
                break;
            case RolesConstant.RegularUser:
                await _regularUserRepository.AddAsync(new RegularUser { ApplicationUserId = user.Id, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, ApplicationUser = user });
                break;
        }
    }



    //public async Task<ServerResponse<AddUserResponseDto>> AddUser(AdminRegisterUserDto registerUserDto)
    //{
    //    var currentUserId = _currentUserService.GetCurrentUserId();
    //    var currentUser = await _userManager.FindByIdAsync(currentUserId);

    //    if (currentUser == null)
    //    {
    //        _logger.LogWarning("Current user not found: {UserId}", currentUserId);
    //        return new ServerResponse<AddUserResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "404",
    //                ResponseMessage = "User not found",
    //                ResponseDescription = "The current user does not exist."
    //            }
    //        };
    //    }

    //    var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, RolesConstant.SuperAdmin);
    //    if (!isSuperAdmin)
    //    {
    //        _logger.LogWarning("Unauthorized user registration attempt by user: {UserId}", currentUserId);
    //        return new ServerResponse<AddUserResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "403",
    //                ResponseMessage = "Unauthorized",
    //                ResponseDescription = "Only SuperAdmin can register users."
    //            }
    //        };
    //    }

    //    if (registerUserDto.Password != registerUserDto.ConfirmPassword)
    //    {
    //        return new ServerResponse<AddUserResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "400",
    //                ResponseMessage = "Password Mismatch",
    //                ResponseDescription = "Passwords do not match."
    //            }
    //        };
    //    }

    //    // Validate the role
    //    var validRoles = new[] { RolesConstant.Creator, RolesConstant.Admin, RolesConstant.RegularUser, RolesConstant.SuperAdmin };

    //    if (string.IsNullOrWhiteSpace(registerUserDto.Role) || !validRoles.Contains(registerUserDto.Role.ToUpper()))
    //    {
    //        return new ServerResponse<AddUserResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "400",
    //                ResponseMessage = "Invalid Role",
    //                ResponseDescription = "The selected role is invalid."
    //            }
    //        };
    //    }

    //    var role = registerUserDto.Role?.ToUpperInvariant();

    //    var applicationUser = new ApplicationUser
    //    {
    //        FirstName = registerUserDto.FirstName,
    //        LastName = registerUserDto.LastName,
    //        UserName = registerUserDto.Email,
    //        Location = registerUserDto.Location,
    //        Email = registerUserDto.Email,
    //        PhoneNumber = registerUserDto.PhoneNumber,
    //        CreatedAt = DateTimeOffset.UtcNow,
    //        UpdatedAt = DateTimeOffset.UtcNow,
    //        PublicId = Guid.NewGuid().ToString(),
    //    };


    //    // Start a transaction
    //    using (var transaction = await _repository.BeginTransactionAsync())
    //    {
    //        try
    //        {
    //            // Create the user
    //            var result = await _userManager.CreateAsync(applicationUser, registerUserDto.Password);
    //            if (!result.Succeeded)
    //            {
    //                _logger.LogError("Error occurred while creating user {UserEmail}", registerUserDto.Email);
    //                return new ServerResponse<AddUserResponseDto>
    //                {
    //                    IsSuccessful = false,
    //                    ErrorResponse = new ErrorResponse
    //                    {
    //                        ResponseCode = "500",
    //                        ResponseMessage = "User Creation Failed",
    //                        ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
    //                    }
    //                };
    //            }

    //            // Add user to role
    //            result = await _userManager.AddToRoleAsync(applicationUser, role);
    //            if (!result.Succeeded)
    //            {
    //                _logger.LogError("Error occurred while assigning role to user {UserEmail}", registerUserDto.Email);
    //                return new ServerResponse<AddUserResponseDto>
    //                {
    //                    IsSuccessful = false,
    //                    ErrorResponse = new ErrorResponse
    //                    {
    //                        ResponseCode = "500",
    //                        ResponseMessage = "Role Assignment Failed",
    //                        ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
    //                    }
    //                };
    //            }

    //            // Add the user to the corresponding role repository
    //            switch (role)
    //            {
    //                case RolesConstant.Admin:
    //                    await _adminRepository.AddAsync(new Admin
    //                    {
    //                        ApplicationUserId = applicationUser.Id,
    //                        CreatedAt = DateTimeOffset.UtcNow,
    //                        UpdatedAt = DateTimeOffset.UtcNow,
    //                        ApplicationUser = applicationUser
    //                    });
    //                    break;

    //                case RolesConstant.SuperAdmin:
    //                    await _superAdminRepository.AddAsync(new SuperAdmin
    //                    {
    //                        ApplicationUserId = applicationUser.Id,
    //                        CreatedAt = DateTimeOffset.UtcNow,
    //                        UpdatedAt = DateTimeOffset.UtcNow,
    //                        ApplicationUser = applicationUser
    //                    });
    //                    break;

    //                case RolesConstant.Creator:
    //                    var creator = new Creator
    //                    {
    //                        ApplicationUserId = applicationUser.Id,
    //                        CreatedAt = DateTimeOffset.UtcNow,
    //                        UpdatedAt = DateTimeOffset.UtcNow,
    //                        ApplicationUser = applicationUser,
    //                        Instagram = null,
    //                        Facebook = null,
    //                        XTwitter = null,
    //                        Tiktok = null,
    //                        // ExclusiveDeals = registerCreatorDto.ExclusiveDeals?.Select(deal => new ExclusiveDeal
    //                        // {
    //                        //     Industry = deal.Industry!,
    //                        //     BrandName = deal.BrandName!,
    //                        //     CreatorId = applicationUser.Id
    //                        // }).ToList() ?? new List<ExclusiveDeal>() // Default to empty list if null
    //                    };
    //                    await _creatorRepository.AddAsync(creator);
    //                    break;

    //                case RolesConstant.RegularUser:
    //                    var regularUser = new RegularUser
    //                    {
    //                        ApplicationUserId = applicationUser.Id,
    //                        CreatedAt = DateTimeOffset.UtcNow,
    //                        UpdatedAt = DateTimeOffset.UtcNow,
    //                        ApplicationUser = applicationUser,
    //                    };
    //                    await _regularUserRepository.AddAsync(regularUser);
    //                    break;
    //            }

    //            // Commit transaction if everything is successful
    //            await transaction.CommitAsync();

    //            var addUserResponse = new AddUserResponseDto
    //            {
    //                FullName = applicationUser.FullName,
    //                UserId = applicationUser.Id,
    //                Email = applicationUser.Email,
    //                ProfilePictureUrl = applicationUser.ImageUrl,
    //                Location = applicationUser.Location,
    //                Role = role,
    //                Message = $"{applicationUser.Email} registration successful."
    //            };

    //            _logger.LogInformation("User {c} registered successfully as {UserRole}", applicationUser.Email, role);
    //            return new ServerResponse<AddUserResponseDto> { IsSuccessful = true, Data = addUserResponse };
    //        }
    //        catch (Exception ex)
    //        {
    //            // Rollback transaction on error
    //            await transaction.RollbackAsync();
    //            _logger.LogError("Error occurred while processing user registration: {ErrorMessage}", ex.Message);
    //            return new ServerResponse<AddUserResponseDto>
    //            {
    //                IsSuccessful = false,
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "500",
    //                    ResponseMessage = "Registration Failed",
    //                    ResponseDescription = "An error occurred during user registration."
    //                }
    //            };
    //        }
    //    }
    //}


    public async Task<ServerResponse<EditUserResponseDto>> EditUserAsync(string userId, EditUserDto editUserDto, IFormFile newProfilePicture)
    {
        try
        {
            // Retrieve user based on provided userId
            var targetUser = await _userManager.FindByIdAsync(userId);

            if (targetUser == null)
            {
                _logger.LogWarning("Target user not found: {UserId}", userId);
                return new ServerResponse<EditUserResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "User not found",
                        ResponseDescription = "The target user does not exist."
                    }
                };
            }

            // Optionally update profile picture if provided
            if (newProfilePicture != null)
            {
                var pictureUpdateResult = await _profileService.UpdateProfilePictureAsync(userId, newProfilePicture);

                if (!pictureUpdateResult.IsSuccessful)
                {
                    _logger.LogError("Failed to update profile picture for user {UserId}", userId);
                    return new ServerResponse<EditUserResponseDto>
                    {
                        IsSuccessful = false,
                        ErrorResponse = pictureUpdateResult.ErrorResponse
                    };
                }

                targetUser.ImageUrl = pictureUpdateResult.Data.NewProfilePictureUrl;
            }

            // Update other user details
            targetUser.FirstName = editUserDto.FirstName ?? targetUser.FirstName;
            targetUser.LastName = editUserDto.LastName ?? targetUser.LastName;
            targetUser.Location = editUserDto.Location ?? targetUser.Location;
            targetUser.PhoneNumber = editUserDto.PhoneNumber ?? targetUser.PhoneNumber;
            targetUser.UpdatedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(targetUser);
            if (!result.Succeeded)
            {
                _logger.LogError("Error updating user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return new ServerResponse<EditUserResponseDto>
                {
                    IsSuccessful = false,
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "500",
                        ResponseMessage = "Update Failed",
                        ResponseDescription = string.Join(", ", result.Errors.Select(e => e.Description))
                    }
                };
            }

            var responseDto = new EditUserResponseDto
            {
                FullName = $"{targetUser.FirstName} {targetUser.LastName}",
                Location = targetUser.Location,
                PhoneNumber = targetUser.PhoneNumber,
                ProfilePictureUrl = targetUser.ImageUrl,
                Message = "User details updated successfully."
            };

            _logger.LogInformation("User {UserId} details updated successfully", userId);
            return new ServerResponse<EditUserResponseDto>
            {
                IsSuccessful = true,
                Data = responseDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing user details for User ID: {UserId}", userId);
            return new ServerResponse<EditUserResponseDto>
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
        user.Location = updateUserDto.location?? user.Location;
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

        var user = await _adminRepository.GetUserWithRelatedEntitiesAsync(userId);
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

        try
        {
            await _adminRepository.DeleteRelatedEntitiesAsync(user);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description);
                _logger.LogError($"Failed to delete user {userId}. Errors: {string.Join(", ", errors)}");

                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Failed to delete user",
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "400",
                        ResponseMessage = "Deletion failed",
                        ResponseDescription = string.Join(", ", errors)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting user with ID: {userId}");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while deleting the user",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Server Error",
                    ResponseDescription = ex.Message
                }
            };
        }
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
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementions;

public class AdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminRepository _adminRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        IAdminRepository adminRepository,
        ICurrentUserService currentUserService,
        ILogger<AdminService> logger)
    {
        _userManager = userManager;
        _adminRepository = adminRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<AddUserResponseDto>> RegisterAdmin(RegisterAdminDto registerAdminDto)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var currentUser = await _userManager.FindByIdAsync(currentUserId);
        var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, RolesConstant.SuperAdmin);

        if (!isSuperAdmin)
            return new Error[] { new("Authorization.Error", "Only SuperAdmin can register Admins") };

        // Ensure password and confirm password match
        if (registerAdminDto.Password != registerAdminDto.ConfirmPassword)
            return new Error[] { new("Registration.Error", "Passwords do not match") };

        // Split the full name
        var names = registerAdminDto.FullName.Split(' ');
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

        // Create ApplicationUser
        var applicationUser = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            UserName = registerAdminDto.Email,
            Email = registerAdminDto.Email,
            PhoneNumber = registerAdminDto.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            //WalletId = GenerateWalletId(),
            PublicId = Guid.NewGuid().ToString(),
        };

        // Create Admin entity and associate the ApplicationUser
        var admin = new Admin
        {
            ApplicationUserId = applicationUser.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ApplicationUser = applicationUser,
        };

        var result = await _userManager.CreateAsync(applicationUser, registerAdminDto.Password);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        result = await _userManager.AddToRoleAsync(applicationUser, RolesConstant.SuperAdmin);
        if (!result.Succeeded)
            return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

        // Save the Admin entity
        await _adminRepository.AddAsync(admin);

        var addUserResponse = new AddUserResponseDto
        {
            UserId = applicationUser.Id,
            Email = applicationUser.Email,
            Role = RolesConstant.Admin,
        Message = "Admin registration successful."
        };

        return Result<RegisterUserResponseDto>.Success(addUserResponse);
    }
}



using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Interfaces;

public interface ISuperAdminService
{
    Task<ServerResponse<List<UserDto>>> GetUsers(string role = null!, bool sortByDate = true);
    Task<ServerResponse<AddUserResponseDto>> AddUser(AdminRegisterUsersDto registerUserDto);  
    Task<ServerResponse<string>> UpdateUser(UpdateUserDto updateUserDto, string userId);
    Task<ServerResponse<string>> DeleteUser(string userId);
    Task<ServerResponse<string>> DisableUser(string userId);
    Task<ServerResponse<string>> ActivateUserAsync(string userId);
    Task<ServerResponse<string>> ToggleUserStatusAsync(string userId);
    Task<ServerResponse<EditUserResponseDto>> EditUserAsync(string userId, EditUserDto editUserDto, IFormFile newProfilePicture);
}

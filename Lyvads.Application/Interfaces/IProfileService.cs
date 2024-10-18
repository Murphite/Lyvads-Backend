
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.AuthDtos;
using static Lyvads.Application.Implementations.ProfileService;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Interfaces;

public interface IProfileService
{
    Task<ServerResponse<EditProfileResponseDto>> EditProfileAsync(EditProfileDto dto, string userId);
    Task<ServerResponse<UpdateProfilePicResponseDto>> UpdateProfilePictureAsync(string userId, IFormFile newProfilePicture);
    Task<ServerResponse<UpdateEmailResponseDto>> InitiateEmailUpdateAsync(string userId, string newEmail);
    Task<ServerResponse<EmailVerificationResponseDto>> VerifyEmailUpdateAsync(string userId, string verificationCode);
    Task<ServerResponse<UpdateLocationResponseDto>> UpdateLocationAsync(UpdateLocationDto dto, string userId);
    Task<ServerResponse<UpdatePhoneNumberResponseDto>> UpdatePhoneNumberAsync(UpdatePhoneNumberDto dto, string userId);
    Task<ServerResponse<UserProfileDto>> GetProfileAsync(string userId);
    Task<ServerResponse<bool>> ValidatePasswordAsync(string email, ValidatePasswordDto validatePasswordDto);
}

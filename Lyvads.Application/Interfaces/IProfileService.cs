
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos.AuthDtos;
using static Lyvads.Application.Implementions.ProfileService;

namespace Lyvads.Application.Interfaces;

public interface IProfileService
{
    Task<Result<EditProfileResponseDto>> EditProfileAsync(EditProfileDto dto, string userId);
    Task<Result<UpdateProfilePicResponseDto>> UpdateProfilePictureAsync(string userId, string newProfilePictureUrl);
    Task<Result<UpdateEmailResponseDto>> InitiateEmailUpdateAsync(string userId, string newEmail);
    Task<Result<EmailVerificationResponseDto>> VerifyEmailUpdateAsync(string userId, string verificationCode);
    Task<Result<UpdateLocationResponseDto>> UpdateLocationAsync(UpdateLocationDto dto, string userId);
    Task<Result<UpdatePhoneNumberResponseDto>> UpdatePhoneNumberAsync(UpdatePhoneNumberDto dto, string userId);
}

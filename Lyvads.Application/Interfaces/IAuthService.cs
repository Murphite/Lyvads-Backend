using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IAuthService
{
    Task<ServerResponse<RegistrationResponseDto>> InitiateRegistration(string email);
    Task<ServerResponse<EmailVerificationResponseDto>> VerifyEmail(string verificationCode);
    Task<ServerResponse<RegisterUserResponseDto>> RegisterUser(RegisterUserDto registerUserDto);
    Task<ServerResponse<RegisterUserResponseDto>> RegisterCreator(RegisterCreatorDto registerCreatorDto);
    Task<ServerResponse<RegisterUserResponseDto>> RegisterSuperAdmin(RegisterSuperAdminDto registerSuperAdminDto);
    Task<ServerResponse<LoginResponseDto>> Login(LoginUserDto loginUserDto);
    Task<ServerResponse<RegistrationResponseDto>> ForgotPassword(ForgotPasswordRequestDto forgotPasswordDto);
    Task<ServerResponse<string>> VerifyVerificationCode(string verificationCode);
    Task<ServerResponse<PasswordResetResponseDto>> ResetPassword(ResetPasswordWithCodeDto resetPasswordDto, string email);
    Task<ServerResponse<RegistrationResponseDto>> AdminForgotPassword(ForgotPasswordRequestDto forgotPasswordDto);
    Task<ServerResponse<string>> VerifyAdminVerificationCode(string verificationCode);
    Task<ServerResponse<PasswordResetResponseDto>> ResetAdminPassword(AdminResetPasswordWithCodeDto resetPasswordDto, string email);
}
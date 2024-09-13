using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.CreatorDtos;

namespace Lyvads.Application.Interfaces;

public interface IAuthService
{
    Task<Result<RegistrationResponseDto>> InitiateRegistration(string email);
    Task<Result<EmailVerificationResponseDto>> VerifyEmail(string verificationCode);
    Task<Result<RegisterUserResponseDto>> RegisterUser(RegisterUserDto registerUserDto);
    Task<Result<RegisterUserResponseDto>> RegisterCreator(RegisterCreatorDto registerCreatorDto);
    //Task<Result<RegisterUserResponseDto>> RegisterAdmin(RegisterAdminDto registerAdminDto);
    Task<Result<RegisterUserResponseDto>> RegisterSuperAdmin(RegisterSuperAdminDto registerSuperAdminDto);
    Task<Result<LoginResponseDto>> Login(LoginUserDto loginUserDto);
    Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    public Task<Result> ForgotPassword(ResetPasswordDto resetPasswordDto);
    public Task<Result> ConfirmEmail(string email, string token);
    public Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    
}
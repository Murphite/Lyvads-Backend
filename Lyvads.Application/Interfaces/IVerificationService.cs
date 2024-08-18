

namespace Lyvads.Application.Interfaces;

public interface IVerificationService
{
    Task SaveVerificationCode(string email, string code);
    Task<string> GetEmailByVerificationCode(string code);
    Task<bool> ValidateVerificationCode(string email, string code);
    Task MarkEmailAsVerified(string email);
    Task<bool> IsEmailVerified(string email);
    Task<string> GetVerifiedEmail(string email);
}

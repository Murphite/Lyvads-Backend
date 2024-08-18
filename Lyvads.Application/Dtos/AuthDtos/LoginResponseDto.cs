
namespace Lyvads.Application.Dtos.AuthDtos;

public class LoginResponseDto
{
    public string Token { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; }

    public LoginResponseDto(string token, string fullName, IList<string> roles, string email)
    {
        Token = token;
        FullName = fullName;
        Roles = roles;
        Email = email;
    }
}

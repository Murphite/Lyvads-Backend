
namespace Lyvads.Application.Dtos.AuthDtos;

public class LoginResponseDto
{
    public string? UserId { get; set; }
    public string? FullName { get; set; }
    public string? AppUserName { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string? Token { get; set; }
    public string? ProfilePicture { get; set; }
    public List<string> Roles { get; set; } 
    public string Role { get; set; } 

    public LoginResponseDto() { }

    public LoginResponseDto(string token, string fullName, List<string> roles, string email)
    {
        Token = token;
        FullName = fullName;
        Roles = roles;
        Email = email;
        Role = roles.FirstOrDefault();
    }
}

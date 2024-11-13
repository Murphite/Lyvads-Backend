
namespace Lyvads.Application.Dtos.AuthDtos;

public class RegisterUserResponseDto
{
    public string? UserId { get; set; }
    public string? AppUserName { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string? Message { get; set; }
    public string? Role { get; set; }
    public string? Token { get; set; }   
    public string? ProfilePictureUrl { get; set; }
}


public class AddUserResponseDto
{

    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Message { get; set; }
    public string? Role { get; set; }
    public string? Location { get; set; }
    public string? ProfilePictureUrl { get; set; }

}
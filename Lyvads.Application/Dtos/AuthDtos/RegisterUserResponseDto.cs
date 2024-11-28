
using Microsoft.AspNetCore.Http;

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

public class EditUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
}


public class EditUserResponseDto
{
    public string? FullName { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Message { get; set; }
}

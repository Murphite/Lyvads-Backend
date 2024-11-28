

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class UserAdDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public string? Status { get; set; }
}


public class AddUserAdDto
{
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}

public class AddUserAdResponseDto
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Status { get; set; }
}

public class EditUserAdDto
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
}


public class EditUserAdResponseDto
{
    public string? AdId { get; set; }
    public string? UserId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

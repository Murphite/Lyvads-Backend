

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class UserAdDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public UserAdStatus Status { get; set; }
}
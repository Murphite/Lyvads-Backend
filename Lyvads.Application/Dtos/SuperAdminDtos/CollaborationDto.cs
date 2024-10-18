
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class CollaborationDto
{
    public string? Id { get; set; }
    public string? RegularUserName { get; set; }
    public string? CreatorName { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset RequestDate { get; set; }
    public CollaborationStatus Status { get; set; }
    public string? Details { get; set; }
    public string? ReceiptUrl { get; set; } 
}



using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class DisputeDto
{
    public string? Id { get; set; }
    public string? RegularUserName { get; set; }
    public string? CreatorName { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset FlaggedDate { get; set; }
    public string? Status { get; set; }
}

public class FetchDisputeDto
{
    public string? DisputeId { get; set; }
    public string? RegularUserFullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DisputeStatus Status { get; set; }
    public DisputeType? DisputeType { get; set; } 
}

public class DisputeDetailsDto
{
    public string? DisputeId { get; set; }
    public string? RegularUserFullName { get; set; }
    public DateTime CreatedAt { get; set; }
   // public string? DisputeType { get; set; } // e.g. "Disputed Video", "Declined Request"
    public DisputeReasons DisputeReason { get; set; } // Reason for dispute
    public string? DisputeMessage { get; set; } // Additional message or details
}

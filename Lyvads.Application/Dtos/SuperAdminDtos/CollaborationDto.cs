
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.SuperAdminDtos;

public class CollaborationDto
{
    public string? Id { get; set; }
    public string? RegularUserName { get; set; }
    public string? RegularUserPic { get; set; }
    public string? CreatorName { get; set; }
    public string? CreatorPic { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RequestAmount { get; set; }
    public DateTimeOffset RequestDate { get; set; }
    public string? Status { get; set; }

    //public string? RequestType { get; set; }
    //public string? Script { get; set; } 
    //public string? VideoUrl { get; set; }
    //public decimal FastTrackFee { get; set; }
}


public class CollaborationDetailsDto
{
    public string? Id { get; set; }
    public string? RegularUserName { get; set; }
    public string? RegularUserPic { get; set; }
    public string? CreatorName { get; set; }
    public string? CreatorPic { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RequestAmount { get; set; }
    public DateTimeOffset RequestDate { get; set; }
    public string? Status { get; set; }
    public string? RequestType { get; set; }
    public string? Script { get; set; }
    public string? VideoUrl { get; set; }
    public decimal FastTrackFee { get; set; }
}

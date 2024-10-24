

using Lyvads.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class GetRequestDto
{
    public string RequestId { get; set; } = default!;
    public string CreatorFullName { get; set; } = default!;
    public string RegularUserFullName { get; set; } = default!;
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetUserRequestDto
{
    public string RequestId { get; set; } = default!;
    public string UserFullName { get; set; } = default!;
    public string CreatorFullName { get; set; } = default!;
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class RequestDetailsDto
{
    public string Script { get; set; } = default!;
    public decimal Amount { get; set; }
    public string CreatorFullName { get; set; } = default!;
    public string RequestId { get; set; } = default!;
    public RequestType RequestType { get; set; }
    public RequestStatus? Status { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; }

}

public class MakeRequestDetailsDto
{
    public string? CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public string? RequestType { get; set; }
    public string? Script { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentSummary { get; set; }
}





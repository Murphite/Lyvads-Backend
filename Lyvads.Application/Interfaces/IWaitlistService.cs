

using Lyvads.Application.Dtos;

namespace Lyvads.Application.Interfaces;

public interface IWaitlistService
{
    Task<Result> AddToWaitlist(string email);
    Task<Result> NotifyWaitlistUsers();
}

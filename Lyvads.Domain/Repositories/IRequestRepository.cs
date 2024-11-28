
using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IRequestRepository
{
    Task<(bool IsSuccess, string ErrorMessage)> CreateRequestAsync(Request request);
    IQueryable<Request> GetRequests();
    Task<Request?> GetRequestByIdAsync(string requestId);
    IQueryable<Request> GetRequestsByUser(string userId);
    IQueryable<Request> GetRequestsForCreator(string creatorId);
    Task<Request> GetRequestByTransactionRefAsync(string trxRef);
    Task UpdateRequestAsync(Request request);
}

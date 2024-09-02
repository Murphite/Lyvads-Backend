
using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IRequestRepository
{
    Task<(bool IsSuccess, string ErrorMessage)> CreateRequestAsync(Request request);
}

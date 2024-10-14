

using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;

namespace Lyvads.Domain.Repositories;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<ServerResponse<Post>> GetByIdAsync(string id);
    Task AddAsync(Post entity);
    Task UpdateAsync(Post entity); 
    Task DeleteAsync(Post entity);
    public IQueryable<Post> GetAllPosts();
}

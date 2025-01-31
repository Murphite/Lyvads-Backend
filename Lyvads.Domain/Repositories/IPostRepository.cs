﻿

using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;
using Lyvads.Shared.DTOs;
using Microsoft.EntityFrameworkCore.Query;

namespace Lyvads.Domain.Repositories;

public interface IPostRepository
{
    Task<List<Comment>> GetRepliesByUserAndPostsAsync(string userId, List<string> postIds);
    Task<IEnumerable<Comment>> GetCommentsByUserAndPostsAsync(string userId, List<string> postIds);
    Task<IEnumerable<Like>> GetLikesByUserAndPostsAsync(string userId, List<string> postIds);
    Task<Post> GetPostWithMediaAsync(string postId);
    Task<PaginatedResponse<Post>> GetPaginatedPostsByCreatorAsync(string creatorId, PaginationFilter paginationFilter);
    Task<Post> GetPostWithDetailsAsync(string postId);
    Task<List<Post>> GetFilteredPostsAsync(List<string> followingIds);
    Task<IEnumerable<Post>> GetAllAsync(
   Func<IQueryable<Post>, IIncludableQueryable<Post, object>>? include = null);
    Task<ServerResponse<Post>> GetByIdAsync(string id);
    Task AddAsync(Post entity);
    Task UpdateAsync(Post entity); 
    Task DeleteAsync(Post entity);
    public IQueryable<Post> GetAllPosts();
    Task<List<Comment>> GetCommentsByPostIdAsync(string postId);
    Task<Post?> GetPostByIdAsync(string postId);
    Task<PaginatorDto<IEnumerable<Post>>> GetPaginatedPostsAsync(List<string> followingIds, PaginationFilter paginationFilter);
}

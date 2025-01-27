using Microsoft.EntityFrameworkCore;
namespace Lyvads.Shared.DTOs;

public class PaginatorDto<T>
{
    public T? PageItems { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int NumberOfPages { get; set; }
}

public class PaginatedResponse<T>
{
    public IEnumerable<T>? Data { get; set; }
    public int PageNumber { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
}


public class PaginationFilter
{
    public PaginationFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize is > 10 or < 1 ? 10 : pageSize;
    }

    public PaginationFilter()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public static class Pagination
{
    public static async Task<PaginatorDto<IEnumerable<TSource>>> PaginateAsync<TSource>(this IQueryable<TSource> queryable,
        PaginationFilter paginationFilter)
        where TSource : class
    {
        var count = await queryable.CountAsync();

        paginationFilter ??= new PaginationFilter();

        var pageResult = new PaginatorDto<IEnumerable<TSource>>
        {
            PageSize = paginationFilter.PageSize,
            CurrentPage = paginationFilter.PageNumber
        };

        pageResult.NumberOfPages = count % pageResult.PageSize != 0
            ? count / pageResult.PageSize + 1
            : count / pageResult.PageSize;

        pageResult.PageItems = await queryable.Skip((pageResult.CurrentPage - 1) * pageResult.PageSize)
            .Take(pageResult.PageSize).ToListAsync();

        return pageResult;
    }
}
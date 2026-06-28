using BusinessObjects.Constants;

namespace BusinessObjects.DTOs;

public sealed class PagedResult<T>
{
    public PagedResult(IEnumerable<T>? items, int pageNumber, int pageSize, int totalCount)
    {
        PageNumber = pageNumber < ValidationRules.DefaultPageNumber
            ? ValidationRules.DefaultPageNumber
            : pageNumber;

        PageSize = pageSize < 1
            ? ValidationRules.DefaultPageSize
            : Math.Min(pageSize, ValidationRules.MaxPageSize);

        TotalCount = Math.Max(0, totalCount);
        Items = items?.ToArray() ?? Array.Empty<T>();
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > ValidationRules.DefaultPageNumber;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Empty(
        int pageNumber = ValidationRules.DefaultPageNumber,
        int pageSize = ValidationRules.DefaultPageSize)
    {
        return new PagedResult<T>(Array.Empty<T>(), pageNumber, pageSize, 0);
    }
}

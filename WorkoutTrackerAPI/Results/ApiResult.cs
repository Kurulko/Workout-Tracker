using System.Linq.Dynamic.Core;
using System.Reflection;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.API.Results;

public class ApiResult<T>
{
    public IList<T> Data { get; private set; }

    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }

    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => (PageIndex + 1) < TotalPages;

    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }

    public string? FilterColumn { get; set; }
    public string? FilterQuery { get; set; }


    private ApiResult(
        IList<T> data,
        int count,
        int pageIndex,
        int pageSize,
        string? sortColumn,
        string? sortOrder,
        string? filterColumn,
        string? filterQuery)
    {
        Data = data;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        SortColumn = sortColumn;
        SortOrder = sortOrder;
        FilterColumn = filterColumn;
        FilterQuery = filterQuery;
    }

    public static async Task<ApiResult<T>> CreateAsync(
        IQueryable<T> source,
        int pageIndex,
        int pageSize,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterQuery) && IsValidProperty(filterColumn))
        {
            source = source.Where(
                string.Format("{0}.ToLower().Contains(@0.ToLower())", filterColumn),
                filterQuery);
        }

        int count = source.Count();

        if (!string.IsNullOrEmpty(sortColumn) && IsValidProperty(sortColumn))
        {
            bool result = sortOrder.TryParseToOrderBy(out OrderBy? _);

            if (!result)
                sortOrder = "ASC";

            source = source.OrderBy($"{sortColumn} {sortOrder}");
        }

        source = source
            .Skip(pageIndex * pageSize)
            .Take(pageSize);

        var data = source.ToList();

        var apiResult = new ApiResult<T>(
            data,
            count,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
        return await Task.FromResult(apiResult);
    }


    public static bool IsValidProperty(string propertyName, bool throwExceptionIfNotFound = true)
    {
        var prop = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (prop == null && throwExceptionIfNotFound)
            throw new NotSupportedException($"ERROR: Property '{propertyName}' doesn't exist.");

        return prop != null;
    }
}

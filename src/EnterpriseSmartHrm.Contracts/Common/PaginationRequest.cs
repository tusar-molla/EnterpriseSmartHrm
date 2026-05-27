namespace EnterpriseSmartHrm.Contracts.Common;

public record PaginationRequest
{
    private const int MaxAllowedPageSize = 100;

    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch
        {
            < 1 => 10,
            > MaxAllowedPageSize => MaxAllowedPageSize,
            _ => value
        };
    }

    public string? SearchTerm { get; init; }

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; } = "asc";
}

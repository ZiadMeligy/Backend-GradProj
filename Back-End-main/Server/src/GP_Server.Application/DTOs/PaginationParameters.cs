namespace GP_Server.Application.DTOs;
public class PaginationParameters
{
    private const int MaxPageSize = 500;
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}


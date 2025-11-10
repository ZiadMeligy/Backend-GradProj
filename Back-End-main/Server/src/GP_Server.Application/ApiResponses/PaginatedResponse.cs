using GP_Server.Application.DTOs.Patients;

namespace GP_Server.Application.ApiResponses;

public class PaginatedResponse<T> : ApiResponse<T>
{
    private T data;

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public PaginatedResponse(T data, int pageNumber, int pageSize, int totalRecords, string? message = null, int statusCode = 200)
        : base(data, message, statusCode)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}

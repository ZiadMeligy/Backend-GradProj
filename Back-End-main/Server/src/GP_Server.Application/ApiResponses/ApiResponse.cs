using Microsoft.AspNetCore.Mvc;

namespace GP_Server.Application.ApiResponses;

public class ApiResponse<T> : IActionResult
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }

    public ApiResponse() { }
    public ApiResponse(T data, string? message = null, int statusCode = 200)
    {
        Success = true;
        Data = data;
        Message = message;
        StatusCode = statusCode;
    }

    public ApiResponse(string message, int statusCode = 200)
    {
        Success = true;
        Data = default;
        Message = message;
        StatusCode = statusCode;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = this.StatusCode
        };
        await objectResult.ExecuteResultAsync(context);
    }
}

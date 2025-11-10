using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GP_Server.Application.ApiResponses;

public class ExceptionResponse : IActionResult
{
        public string? Message { get; set; }
        public int StatusCode { get; set; }

        public ExceptionResponse() { }
        public ExceptionResponse(Exception exception)
        {
            Message = exception is CustomException customException
                ? customException.Message
                : exception.Message + "\n Inner Message: \n" + (exception.InnerException != null ? exception.InnerException.Message : "No Inner Exception");

            StatusCode = exception is CustomException customEx
                ? customEx.StatusCode
                : StatusCodes.Status500InternalServerError;
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

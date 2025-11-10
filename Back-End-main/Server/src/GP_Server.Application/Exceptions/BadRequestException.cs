namespace GP_Server.Application.Exceptions;

public class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(message, 400)
    {
    }
}


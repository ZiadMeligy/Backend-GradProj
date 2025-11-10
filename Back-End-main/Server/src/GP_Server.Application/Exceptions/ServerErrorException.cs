namespace GP_Server.Application.Exceptions;

public class ServerErrorException : CustomException
{
    public ServerErrorException(string message) : base(message, 500)
    {
    }
}

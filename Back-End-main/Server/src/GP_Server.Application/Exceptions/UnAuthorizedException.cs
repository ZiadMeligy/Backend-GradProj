using System;

namespace GP_Server.Application.Exceptions;

public class UnAuthorizedException : CustomException
{
    public UnAuthorizedException(string message) : base(message, 401)
    {
    }
}

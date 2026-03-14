using System.Security.Authentication;

namespace TimeTracker.Business.Common.Exceptions.Api.Auth;

public class InvalidTokenException: AuthenticationException
{
    public InvalidTokenException(
        string message = "Invalid token"
    ) : base(message)
    {
    }
}

using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api.Auth;

public class UserNotFoundException : Exception, IDomainException
{
    public UserNotFoundException(string? message = null) : base(
        string.IsNullOrEmpty(message) ? "User was not found" : message
    )
    {
    }
}

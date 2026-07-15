using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api.Auth;

public class IncorrectPasswordException : Exception, IDomainException
{
    public IncorrectPasswordException()
        : base("The current password is incorrect.")
    {
    }
}

using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api;

public class ForbiddenException : Exception, IDomainException
{
    public ForbiddenException(string message = "") : base(message)
    {
    }
}

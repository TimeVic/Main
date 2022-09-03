using Domain.Abstractions;

namespace TimeTracker.Business.Exceptions.Api
{
    public class ValidationException : Exception, IDomainException
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}

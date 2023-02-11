using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api
{
    public class ValidationException : Exception, IDomainException
    {
        public ValidationException(string message = "") : base(message)
        {
        }
    }
}

using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api
{
    public class HasNoAccessException : Exception, IDomainException
    {
        public HasNoAccessException(string message = "") : base(message)
        {
        }
    }
}

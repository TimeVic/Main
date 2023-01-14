using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api
{
    public class IncorrectFileException : Exception, IDomainException
    {
        public IncorrectFileException(string message = "") : base(message)
        {
        }
    }
}

using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api
{
    public class RecordIsExistsException : Exception, IDomainException
    {
        public RecordIsExistsException(string message = "Record is exists") : base(message)
        {
        }
    }
}

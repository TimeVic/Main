using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Common
{
    public class RecordExpiredException : Exception, IDomainException
    {
        public RecordExpiredException(string message = "") : base(
            string.IsNullOrEmpty(message)
                ? $"Record is expired"
                : message
        )
        {
        }

        public RecordExpiredException(long id) : this(id.ToString())
        {
        }
    }
}

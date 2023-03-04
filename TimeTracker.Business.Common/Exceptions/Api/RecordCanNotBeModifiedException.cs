using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api
{
    public class RecordCanNotBeModifiedException : Exception, IDomainException
    {
        public RecordCanNotBeModifiedException(string message = "Record can not be modified") : base(message)
        {
        }
    }
}

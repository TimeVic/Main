using Domain.Abstractions;

namespace TimeTracker.Business.Orm.Exceptions
{
    public class DataInconsistentException: Exception, IDomainException
    {
        public DataInconsistentException(string error = $"Data is inconsistent") : base(error)
        {
        }
    }
}

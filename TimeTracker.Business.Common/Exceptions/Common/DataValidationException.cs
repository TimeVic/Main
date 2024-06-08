using Domain.Abstractions;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Exceptions.Common
{
    public class DataValidationException : Exception, IDomainException
    {
        public DataValidationException(): this(RG.Error_DataValidationException)
        {
        }

        public DataValidationException(string message) : base(message)
        {
        }
    }
}

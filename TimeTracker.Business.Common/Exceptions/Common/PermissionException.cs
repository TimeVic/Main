using Domain.Abstractions;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Exceptions.Common
{
    public class PermissionException : Exception, IDomainException
    {
        public PermissionException(): this(RG.Error_UserHasNotPermissions)
        {
        }

        public PermissionException(string message) : base(message)
        {
        }
    }
}

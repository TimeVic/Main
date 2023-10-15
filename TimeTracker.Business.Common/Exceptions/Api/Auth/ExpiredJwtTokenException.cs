using System.Security.Authentication;
using Domain.Abstractions;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Exceptions.Api.Auth
{
    public class ExpiredJwtTokenException : AuthenticationException, IDomainException
    {
        public ExpiredJwtTokenException(): this(RG.Error_ExpiredJwtToken)
        {
        }

        public ExpiredJwtTokenException(string message) : base(message)
        {
        }
    }
}

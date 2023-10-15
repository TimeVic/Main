using System.Security.Authentication;
using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api.Auth
{
    public class UserNotAuthorizedException : AuthenticationException, IDomainException
    {

    }
}

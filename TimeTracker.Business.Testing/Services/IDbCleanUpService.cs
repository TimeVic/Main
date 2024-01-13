using Domain.Abstractions;

namespace TimeTracker.Business.Testing.Services;

public interface IDbCleanUpService: IDomainService
{
    Task CleanUp();
}

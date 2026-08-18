using Domain.Abstractions;

namespace TimeTracker.Business.Common.Exceptions.Api;

public class PublicSharedReportNotFoundException : Exception, INotFoundDomainException
{
    public PublicSharedReportNotFoundException() : base("Shared report was not found")
    {
    }
}

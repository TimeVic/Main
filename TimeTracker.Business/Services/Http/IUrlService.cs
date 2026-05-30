using Domain.Abstractions;

namespace TimeTracker.Business.Services.Http;

public interface IUrlService: IDomainService
{
    string ToFrontendAbsoluteUrl(string relativePath);
}

using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage.Client;

public interface IFileStorageGarageClient: IFileStorageClient, IDomainService
{
}

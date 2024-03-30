using System.Security.Authentication;
using TimeTracker.Business.Orm.Dao.FileStorage;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.FileStorage.Services.Api;

public class FileStorageSecurityService: IFileStorageSecurityService
{
    private readonly IFileStorageRequestService _requestService;
    private readonly IFileStorageAccessKeyDao _accessKeyDao;

    public FileStorageSecurityService(
        IFileStorageRequestService requestService,
        IFileStorageAccessKeyDao accessKeyDao
    )
    {
        _requestService = requestService;
        _accessKeyDao = accessKeyDao;
    }

    public UserEntity GetCurrentUser()
    {
        throw new NotImplementedException();
    }

    public async Task CheckIsAuthenticated()
    {
        var accessKey = await _accessKeyDao.GetByKey(_requestService.GetApiKey());
        if (accessKey == null || accessKey.SecretKey != _requestService.GetApiSecret())
        {
            throw new AuthenticationException();
        }
    }
}

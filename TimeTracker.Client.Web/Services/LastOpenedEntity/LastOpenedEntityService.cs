using Blazored.LocalStorage;

namespace TimeTracker.Client.Web.Services.LastOpenedEntity;

public class LastOpenedEntityService: ILastOpenedEntityService
{
    private const string StorageKeyPrefix = "timevic.last-opened";

    private readonly ILocalStorageService _localStorage;

    public LastOpenedEntityService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public Task<Guid?> GetLastOpenedIdAsync(Guid workspaceId, LastOpenedEntityType entityType)
    {
        return _localStorage.GetItemAsync<Guid?>(GetStorageKey(workspaceId, entityType)).AsTask();
    }

    public Task SetLastOpenedIdAsync(Guid workspaceId, LastOpenedEntityType entityType, Guid entityId)
    {
        return _localStorage.SetItemAsync(GetStorageKey(workspaceId, entityType), entityId).AsTask();
    }

    private static string GetStorageKey(Guid workspaceId, LastOpenedEntityType entityType)
    {
        return $"{StorageKeyPrefix}.{entityType}.{workspaceId}";
    }
}

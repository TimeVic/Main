namespace TimeTracker.Client.Web.Services.LastOpenedEntity;

public enum LastOpenedEntityType
{
    TaskList
}

public interface ILastOpenedEntityService
{
    Task<Guid?> GetLastOpenedIdAsync(Guid workspaceId, LastOpenedEntityType entityType);

    Task SetLastOpenedIdAsync(Guid workspaceId, LastOpenedEntityType entityType, Guid entityId);
}

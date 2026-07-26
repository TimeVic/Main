using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;

namespace TimeTracker.Client.Core.Store.Project;

public record struct UpdateAction(UpdateRequest Request);

public record struct AddAction(AddRequest Request);

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(GetListResponse Response);

public record struct SetListItemAction(ProjectDto Project);

public record struct DeleteItemAction(ProjectDto Project);

public record struct DeleteListItemAction(Guid ProjectId);

public record struct SetSelectedAction(ProjectDto Project);

public record struct SetIsSavingAction(bool IsSaving);

public record struct SetProjectIsListLoading(bool IsLoading);

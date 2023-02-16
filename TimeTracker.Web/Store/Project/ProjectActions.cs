using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;

namespace TimeTracker.Web.Store.Project;

public record struct LoadListAction(bool IsReload = false);

public record struct SetProjectListItemsAction(GetListResponse Response);

public record struct SetProjectListItemAction(ProjectDto Project);

public record struct SetProjectIsListLoading(bool IsLoading);

public record struct AddEmptyProjectListItemAction();

public record struct RemoveEmptyProjectListItemAction();

public record struct SaveEmptyProjectListItemAction();

using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Web.Store.Workspace;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(PaginatedListDto<WorkspaceDto> Response);

public record struct SetListItemAction(WorkspaceDto Workspace);

public record struct SetIsListLoading(bool IsLoading);

#region Effects

public record struct UpdateWorkspaceAction(UpdateRequest Model);

#endregion

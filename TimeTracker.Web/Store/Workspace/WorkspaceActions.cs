using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Store.Workspace;

public record struct LoadListAction(bool IsReload = false);

public record struct SetListItemsAction(PaginatedListDto<WorkspaceDto> Response);

public record struct SetIsListLoading(bool IsLoading);

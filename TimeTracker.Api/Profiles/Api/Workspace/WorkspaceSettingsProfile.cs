using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Profiles.Api.Workspace;

public class WorkspaceSettingsProfile : Profile
{
    public WorkspaceSettingsProfile()
    {
        CreateMap<WorkspaceSettingsRedmineEntity, WorkspaceSettingsRedmineDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new WorkspaceSettingsRedmineDto
            {
                Url = src.Url,
                ApiKey = src.ApiKey,
                RedmineUserId = src.RedmineUserId,
                ActivityId = src.ActivityId,
                IsActive = src.IsActive
            });
        CreateMap<WorkspaceSettingsClickUpEntity, WorkspaceSettingsClickUpDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new WorkspaceSettingsClickUpDto
            {
                SecurityKey = src.SecurityKey ?? string.Empty,
                TeamId = src.TeamId ?? string.Empty,
                IsCustomTaskIds = src.IsCustomTaskIds,
                IsFillTimeEntryWithTaskDetails = src.IsFillTimeEntryWithTaskDetails,
                IsActive = src.IsActive
            });
        CreateMap<WorkspaceSettingsJiraEntity, WorkspaceSettingsJiraDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new WorkspaceSettingsJiraDto
            {
                Url = src.Url,
                ApiKey = src.ApiKey,
                UserName = src.UserName,
                IsActive = src.IsActive
            });
    }
}

using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Api.Profiles.Workspace;

public class WorkspaceMemberProfile : Profile
{
    public WorkspaceMemberProfile()
    {
        CreateMap<WorkspaceMembershipEntity, WorkspaceMembershipDto>();
    }
}

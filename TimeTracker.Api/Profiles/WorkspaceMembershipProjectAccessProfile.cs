using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Api.Profiles;

public class WorkspaceMembershipProjectAccessProfile : Profile
{
    public WorkspaceMembershipProjectAccessProfile()
    {
        CreateMap<WorkspaceMembershipProjectAccessEntity, WorkspaceMembershipProjectAccessDto>();
    }
}

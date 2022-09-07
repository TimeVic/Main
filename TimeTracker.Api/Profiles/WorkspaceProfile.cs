using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles;

public class WorkspaceProfile : Profile
{
    public WorkspaceProfile()
    {
        CreateMap<WorkspaceEntity, WorkspaceDto>();
    }
}

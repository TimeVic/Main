using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Api.Profiles.Api.Workspace;

public class WorkspaceMemberProjectAccessProfile : Profile
{
    public WorkspaceMemberProjectAccessProfile()
    {
        CreateMap<WorkspaceMemberProjectAccessEntity, WorkspaceMemberProjectAccessDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new WorkspaceMemberProjectAccessDto
            {
                HourlyRate = src.HourlyRate,
                Project = mapper.Mapper.Map<ProjectDto>(src.Project)
            });
    }
}

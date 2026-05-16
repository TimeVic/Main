using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Api.Profiles.Api.Workspace;

public class WorkspaceMemberProfile : Profile
{
    public WorkspaceMemberProfile()
    {
        CreateMap<WorkspaceMemberEntity, WorkspaceMemberDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new WorkspaceMemberDto
            {
                Id = src.Id,
                Access = src.Access,
                User = mapper.Mapper.Map<UserDto>(src.User),
                ProjectAccesses = mapper.Mapper.Map<ICollection<WorkspaceMemberProjectAccessDto>>(src.ProjectAccesses.ToList())
            });
    }
}

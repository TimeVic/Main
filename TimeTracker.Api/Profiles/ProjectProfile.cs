using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectEntity, ProjectDto>();
        CreateMap<UpdateRequest, ProjectEntity>();
    }
}

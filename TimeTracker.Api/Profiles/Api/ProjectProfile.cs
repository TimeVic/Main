using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectEntity, ProjectDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var client = mapper.Mapper.Map<ClientDto>(src.Client);
                return new ProjectDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    IsBillableByDefault = src.IsBillableByDefault,
                    DefaultHourlyRate = src.DefaultHourlyRate,
                    IsArchived = src.IsArchived,
                    Client = client,
                };
            });
        CreateMap<UpdateRequest, ProjectEntity>();
    }
}

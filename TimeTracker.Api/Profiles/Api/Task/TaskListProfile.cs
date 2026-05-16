using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Api.Task;

public class TaskListProfile : Profile
{
    public TaskListProfile()
    {
        CreateMap<TaskListEntity, TaskListDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new TaskListDto
            {
                Id = src.Id,
                Name = src.Name,
                Project = mapper.Mapper.Map<TimeTracker.Api.Shared.Dto.Entity.ProjectDto>(src.Project)
            });
        CreateMap<TaskListEntity, TaskListForListDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new TaskListForListDto
            {
                Id = src.Id,
                Name = src.Name,
                Project = mapper.Mapper.Map<TimeTracker.Api.Shared.Dto.Entity.ProjectDto>(src.Project),
                TasksCount = 0
            })
            .ForMember(destination => destination.TasksCount, options => options.Ignore());
    }
}

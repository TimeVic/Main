using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Api.Task;

public class TaskListProfile : Profile
{
    public TaskListProfile()
    {
        CreateMap<TaskListEntity, TaskListDto>();
        CreateMap<TaskListEntity, TaskListForListDto>()
            .IncludeBase<TaskListEntity, TaskListDto>()
            .ForMember(destination => destination.TasksCount, options => options.Ignore());
    }
}

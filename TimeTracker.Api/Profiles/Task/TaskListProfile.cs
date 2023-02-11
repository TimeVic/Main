using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Task;

public class TaskListProfile : Profile
{
    public TaskListProfile()
    {
        CreateMap<TaskListEntity, TaskListDto>();
    }
}

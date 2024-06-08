using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Task;

public class TaskListProfile : Profile
{
    public TaskListProfile()
    {
        CreateMap<TaskListEntity, TaskListDto>();
    }
}

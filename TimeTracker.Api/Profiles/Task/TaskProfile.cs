using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Orm.Dto.Task;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Task;

public class TaskProfile : Profile
{
    public TaskProfile()
    {
        CreateMap<TaskEntity, TaskDto>();
        CreateMap<UpdateRequest, TaskEntity>();
        CreateMap<GetListFilterRequest, GetTasksFilterDto>();
    }
}

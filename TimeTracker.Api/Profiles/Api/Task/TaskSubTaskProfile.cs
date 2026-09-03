using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Api.Task;

public class TaskSubTaskProfile : Profile
{
    public TaskSubTaskProfile()
    {
        CreateMap<TaskSubTaskEntity, TaskSubTaskDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new TaskSubTaskDto
            {
                Id = src.Id,
                TaskId = src.Task.Id,
                Title = src.Title,
                IsCompleted = src.IsCompleted,
                PositionIndex = src.PositionIndex,
                CreatedAt = src.CreatedAt,
                UpdatedAt = src.UpdatedAt
            });
    }
}

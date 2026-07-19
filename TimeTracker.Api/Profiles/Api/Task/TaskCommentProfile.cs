using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Api.Task;

public class TaskCommentProfile : Profile
{
    public TaskCommentProfile()
    {
        CreateMap<TaskCommentEntity, TaskCommentDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var user = mapper.Mapper.Map<UserDto>(src.User);
                var attachments = mapper.Mapper.Map<List<StoredFileDto>>(src.Attachments.ToList());
                var watchers = mapper.Mapper.Map<List<UserDto>>(src.Watchers.ToList());
                return new TaskCommentDto
                {
                    Id = src.Id,
                    Comment = src.Comment,
                    UpdatedAt = src.UpdatedAt,
                    CreatedAt = src.CreatedAt,
                    User = user,
                    Attachments = attachments,
                    Watchers = watchers,
                    Task = new TaskDto
                    {
                        Id = src.Task.Id,
                        TaskId = src.Task.TaskId
                    }
                };
            });
    }
}

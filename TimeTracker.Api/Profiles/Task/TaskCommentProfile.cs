using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Task;

public class TaskCommentProfile : Profile
{
    public TaskCommentProfile()
    {
        CreateMap<TaskCommentEntity, TaskCommentDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var user = mapper.Mapper.Map<UserDto>(src.User);
                var attachments = mapper.Mapper.Map<List<StoredFileDto>>(src.Attachments.ToList());
                var task = mapper.Mapper.Map<TaskDto>(src.Task);
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
                    Task = task 
                };
            });
    }
}

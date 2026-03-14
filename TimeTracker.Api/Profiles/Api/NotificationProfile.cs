using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Notifications;

namespace TimeTracker.Api.Profiles.Api;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationEntity, NotificationDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var performedUser = mapper.Mapper.Map<UserDto>(src.PerformedUser);
                var receiverUser = mapper.Mapper.Map<UserDto>(src.ReceiverUser);
                var task = mapper.Mapper.Map<TaskDto>(src.Task);
                var taskComment = mapper.Mapper.Map<TaskCommentDto>(src.TaskComment);
                return new NotificationDto
                {
                    Id = src.Id,
                    Type = src.Type,
                    IsRead = src.IsRead,
                    CreatedAt = src.CreatedAt,
                    Comment = src.Comment,
                    PerformedUser = performedUser,
                    ReceiverUser = receiverUser,
                    Task = task,
                    TaskComment = taskComment 
                };
            });
    }
}

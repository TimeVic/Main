using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities.Notifications;

namespace TimeTracker.Api.Profiles;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationEntity, NotificationDto>()
            .ForPath(x => x.Task.Attachments, opt => opt.Ignore())
            .ForPath(x => x.Task.TaskList.Project, opt => opt.Ignore());
    }
}

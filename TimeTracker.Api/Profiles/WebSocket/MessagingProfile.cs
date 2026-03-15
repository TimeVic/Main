using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Messaging;

namespace TimeTracker.Api.Profiles.WebSocket;

public class MessagingProfile : Profile
{
    public MessagingProfile()
    {        
        CreateMap<MessagingChannelEntity, MessagingChannelDto>()
            .IgnoreAllAndConstructUsing((e, mapper) => new MessagingChannelDto
            {
                Id = e.Id,
                Type = e.Type,
                Slug = e.Slug,
                Workspace = mapper.Mapper.Map<WorkspaceDto>(e.Workspace),
                CreatedBy = mapper.Mapper.Map<UserDto>(e.CreatedBy),
                User = mapper.Mapper.Map<UserDto>(e.User),
            });
        CreateMap<MessagingMessageEntity, MessagingMessageDto>()
            .IgnoreAllAndConstructUsing((e, mapper) => new MessagingMessageDto
            {
                Id = e.Id,
                Text = e.Text,
                CreatedAt = e.CreatedAt,
                Channel = mapper.Mapper.Map<MessagingChannelDto>(e.Channel),
                CreatedBy = mapper.Mapper.Map<UserDto>(e.CreatedBy),
            });
        CreateMap<MessagingCounterEntity, MessagingMessageCountDto>()
            .IgnoreAllAndConstructUsing((e, mapper) => new MessagingMessageCountDto
            {
                Counter = e.Counter,
                Channel = mapper.Mapper.Map<MessagingChannelDto>(e.Channel),
            });
            
        CreateMap<MessagingActivityEntity, MessagingChannelActivityDto>()
            .IgnoreAllAndConstructUsing((e, mapper) =>
            {
                var user = mapper.Mapper.Map<UserDto>(e.User);
                var chanel = mapper.Mapper.Map<MessagingChannelDto>(e.Channel);
                return new MessagingChannelActivityDto
                {
                    User = user,
                    Channel = chanel, 
                    IsWriting = e.IsWriting,
                };
            });
    }
}

using Newtonsoft.Json;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Business.Common.Constants.Messaging;

namespace TimeTracker.Api.Shared.Dto.Entity.Messaging;

public class MessagingChannelDto: BaseDto
{
    public virtual MessagingChannelType Type { get; set; }
    public virtual required string Name { get; set; }
    
    public virtual required WorkspaceDto Workspace { get; set; }
    public virtual required UserDto CreatedBy { get; set; }
}

using TimeTracker.Api.Shared.Constants.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity.Messaging;

public class MessagingMessageDto: BaseDto
{   
    public required string Text { get; set; }
    
    public required DateTime CreatedAt { get; set; }
    
    public HubMessageDirectionEnum Direction { get; set; }
    
    public required MessagingChannelDto Channel { get; set; }
    
    public required UserDto CreatedBy { get; set; }
}

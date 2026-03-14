namespace TimeTracker.Api.Shared.Dto.Entity.Messaging;

public class MessagingChannelActivityDto
{
    public MessagingChannelDto? Channel { get; set; }
    
    public UserDto? User { get; set; }
    
    public bool IsWriting { get; set; }
}

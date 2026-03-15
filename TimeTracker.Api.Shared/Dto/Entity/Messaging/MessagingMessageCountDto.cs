namespace TimeTracker.Api.Shared.Dto.Entity.Messaging;

public class MessagingMessageCountDto
{
    public required long Counter { get; set; }
    
    public required MessagingChannelDto Channel { get; set; }
}

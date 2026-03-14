namespace TimeTracker.Api.Shared.Dto.Entity.Messaging;

public class MessagingMessageCountDto
{
    public required long Counter { get; set; }
    
    public MessagingChannelDto? Channel { get; set; }
}

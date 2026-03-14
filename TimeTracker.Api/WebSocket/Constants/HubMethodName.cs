namespace TimeTracker.Api.WebSocket.Constants;

public class HubMethodName
{
    public static string MessageCreated = "MessageCreated";
    public static string MessageCounterUpdated = "MessageCounterUpdated";
    
    public static string ChannelActivityUpdated = "ChannelActivityUpdated";
    
    public static string PongResponse = "PongResponse";
    public static string LoadChannelsResponse = "LoadChannelsResponse";
    public static string LoadMessagesResponse = "LoadMessagesResponse";
    public static string LoadCountersResponse = "LoadCountersResponse";
    public static string ChannelCreated = "ChannelCreated";
}

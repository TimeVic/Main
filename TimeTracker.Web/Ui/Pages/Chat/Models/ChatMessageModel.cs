namespace TimeTracker.Web.Ui.Pages.Chat.Models;

public class ChatMessageModel
{
    public string Id { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public string? ChannelId { get; set; }

    public List<string> DirectUserIds { get; set; } = new();
}

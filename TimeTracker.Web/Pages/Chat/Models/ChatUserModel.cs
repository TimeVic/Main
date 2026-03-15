namespace BlazorApp.Models;

public class ChatUserModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsOnline { get; set; }
}

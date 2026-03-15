using BlazorApp.Models;

namespace TimeTracker.Web.Pages.Chat;

public partial class Index
{
    protected override async Task OnInitializedAsync()
    {
        // IsRedirectIfNotLoggedIn = false;
        await base.OnInitializedAsync();
    }
    
    private readonly List<ChatUserModel> users = new()
    {
        new() { Id = "u1", Name = "Alex Carter", IsOnline = true },
        new() { Id = "u2", Name = "Mia Chen", IsOnline = true },
        new() { Id = "u3", Name = "Noah Patel", IsOnline = false },
        new() { Id = "u4", Name = "Sofia Lopez", IsOnline = true }
    };

    private readonly List<ChatChannelModel> channels = new()
    {
        new() { Id = "c1", Name = "general", Description = "Team-wide updates and daily chat", MemberIds = new() { "u1", "u2", "u3", "u4" } },
        new() { Id = "c2", Name = "design", Description = "UI reviews and component feedback", MemberIds = new() { "u1", "u2", "u4" } },
        new() { Id = "c3", Name = "engineering", Description = "Feature work and implementation notes", MemberIds = new() { "u1", "u3", "u4" } }
    };

    private readonly List<ChatMessageModel> messages = new()
    {
        new() { Id = "m1", SenderId = "u2", Text = "Morning team — I pushed the updated mockups.", SentAt = DateTime.Today.AddHours(9).AddMinutes(5), ChannelId = "c1" },
        new() { Id = "m2", SenderId = "u1", Text = "Nice, I’ll review them before standup.", SentAt = DateTime.Today.AddHours(9).AddMinutes(11), ChannelId = "c1" },
        new() { Id = "m3", SenderId = "u4", Text = "Header spacing looks much better on mobile now.", SentAt = DateTime.Today.AddHours(9).AddMinutes(18), ChannelId = "c2" },
        new() { Id = "m4", SenderId = "u2", Text = "I also simplified the card states for empty conversations.", SentAt = DateTime.Today.AddHours(9).AddMinutes(22), ChannelId = "c2" },
        new() { Id = "m5", SenderId = "u3", Text = "I’m wiring the message composer this afternoon.", SentAt = DateTime.Today.AddHours(10).AddMinutes(2), ChannelId = "c3" },
        new() { Id = "m6", SenderId = "u1", Text = "Perfect — I already have the local state model ready.", SentAt = DateTime.Today.AddHours(10).AddMinutes(10), ChannelId = "c3" },
        new() { Id = "m7", SenderId = "u1", Text = "Can you send over the revised CTA copy?", SentAt = DateTime.Today.AddHours(11).AddMinutes(8), DirectUserIds = new() { "u1", "u2" } },
        new() { Id = "m8", SenderId = "u2", Text = "Yes — I’ll drop it here in a few minutes.", SentAt = DateTime.Today.AddHours(11).AddMinutes(12), DirectUserIds = new() { "u1", "u2" } },
        new() { Id = "m9", SenderId = "u4", Text = "Do we want a compact sidebar layout for tablets too?", SentAt = DateTime.Today.AddHours(11).AddMinutes(20), DirectUserIds = new() { "u1", "u4" } },
        new() { Id = "m10", SenderId = "u1", Text = "Yes, let’s keep the content density high but readable.", SentAt = DateTime.Today.AddHours(11).AddMinutes(24), DirectUserIds = new() { "u1", "u4" } }
    };

    private string activeUserId = "u1";
    private string selectedView = "channel";
    private string selectedChannelId = "c1";
    private string selectedDirectUserId = "u2";
    private string draftMessage = string.Empty;

    private ChatUserModel ActiveUser => users.First(u => u.Id == activeUserId);
    private IEnumerable<ChatUserModel> DirectMessageContacts => users.Where(u => u.Id != activeUserId);
    private ChatChannelModel? ActiveChannel => channels.FirstOrDefault(c => c.Id == selectedChannelId);
    private ChatUserModel? ActiveDirectContact => users.FirstOrDefault(u => u.Id == selectedDirectUserId);
    private IEnumerable<ChatMessageModel> VisibleMessages => selectedView == "channel"
        ? messages.Where(m => m.ChannelId == selectedChannelId).OrderBy(m => m.SentAt)
        : messages.Where(m => m.DirectUserIds.Contains(activeUserId) && m.DirectUserIds.Contains(selectedDirectUserId)).OrderBy(m => m.SentAt);

    private string ActiveTitle => selectedView == "channel"
        ? $"#{ActiveChannel?.Name}"
        : ActiveDirectContact?.Name ?? "Direct message";

    private string ActiveSubtitle => selectedView == "channel"
        ? ActiveChannel?.Description ?? "Channel conversation"
        : ActiveDirectContact?.IsOnline == true ? "Direct message · Online" : "Direct message · Away";

    private void SelectChannel(string channelId)
    {
        selectedView = "channel";
        selectedChannelId = channelId;
    }

    private void SelectDirect(string userId)
    {
        selectedView = "direct";
        selectedDirectUserId = userId;
    }

    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(draftMessage))
        {
            return;
        }

        var message = new ChatMessageModel
        {
            Id = $"m{messages.Count + 1}",
            SenderId = activeUserId,
            Text = draftMessage.Trim(),
            SentAt = DateTime.Now,
            ChannelId = selectedView == "channel" ? selectedChannelId : null,
            DirectUserIds = selectedView == "direct" ? new List<string> { activeUserId, selectedDirectUserId } : new List<string>()
        };

        messages.Add(message);
        draftMessage = string.Empty;
    }

    private void SeedReply()
    {
        var replySenderId = selectedView == "channel"
            ? channels.First(c => c.Id == selectedChannelId).MemberIds.First(id => id != activeUserId)
            : selectedDirectUserId;

        var replyText = selectedView == "channel"
            ? "This looks good — let’s keep the flow lightweight for the demo."
            : "Got it, I’m on it.";

        messages.Add(new ChatMessageModel
        {
            Id = $"m{messages.Count + 1}",
            SenderId = replySenderId,
            Text = replyText,
            SentAt = DateTime.Now,
            ChannelId = selectedView == "channel" ? selectedChannelId : null,
            DirectUserIds = selectedView == "direct" ? new List<string> { activeUserId, selectedDirectUserId } : new List<string>()
        });
    }

    private ChatUserModel GetUserById(string userId)
    {
        return users.First(u => u.Id == userId);
    }

    private string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpperInvariant();
        }

        return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }

    private string GetDirectPreview(string userId)
    {
        var preview = messages
            .Where(m => m.DirectUserIds.Contains(activeUserId) && m.DirectUserIds.Contains(userId))
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        return preview?.Text ?? "Start a new conversation";
    }

    private string GetNavButtonClass(bool isActive)
    {
        return isActive
            ? "flex w-full items-center justify-between rounded-2xl border border-cyan-200 bg-cyan-50 px-3 py-3 text-left transition"
            : "flex w-full items-center justify-between rounded-2xl border border-transparent bg-white px-3 py-3 text-left transition hover:border-slate-200 hover:bg-slate-50";
    }
}

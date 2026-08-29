using LumexUI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using TimeTracker.Client.Web.Constants;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;

public partial class SupportModal
{
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    [Parameter]
    public bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    private LumexModal? _modal;

    private string SupportEmail => SiteMetadata.SupportEmail;

    private string EmailMailtoHref => $"mailto:{SupportEmail}?subject=[TimeVic%20Beta]%20Bug%20Report%20/%20Feedback";

    private string DiscordUrl => Configuration["SocialLinks:Discord"] ?? "https://discord.gg";

    private async Task OnCloseModal()
    {
        IsOpened = false;
        await IsOpenedChanged.InvokeAsync(false);
    }
}

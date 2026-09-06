using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Constants;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;

public partial class SupportModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private string SupportEmail => SiteMetadata.SupportEmail;

    private string EmailMailtoHref => $"mailto:{SupportEmail}?subject=[TimeVic%20Beta]%20Bug%20Report%20/%20Feedback";

    private string DiscordUrl => Configuration["SocialLinks:Discord"] ?? "https://discord.gg";

    private async Task OnCloseModal()
    {
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
    }
}

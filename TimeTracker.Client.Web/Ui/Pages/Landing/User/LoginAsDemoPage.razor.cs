using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Web.Services;

namespace TimeTracker.Client.Web.Ui.Pages.Landing.User;

public partial class LoginAsDemoPage
{
    [Parameter]
    [SupplyParameterFromQuery(Name = "mode")]
    public string? Mode { get; set; }

    [Inject]
    private ApiService _apiService { get; set; } = default!;

    [Inject]
    private NavigationManager _navigationManager { get; set; } = default!;

    [Inject]
    private IAuthorizationService _authorizationService { get; set; } = default!;

    private bool _isError;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await base.OnAfterRenderAsync(firstRender);
        await LoadDemoAsync();
    }

    private WorkspaceMode GetRequestedMode()
    {
        if (!string.IsNullOrWhiteSpace(Mode) && Enum.TryParse<WorkspaceMode>(Mode, true, out var parsedMode))
        {
            return parsedMode;
        }

        try
        {
            var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("mode", out var queryModeStr))
            {
                if (Enum.TryParse<WorkspaceMode>(queryModeStr.ToString(), true, out var fromQueryMode))
                {
                    return fromQueryMode;
                }
            }
        }
        catch
        {
            // fallback to default
        }

        return WorkspaceMode.Solo;
    }

    private async Task LoadDemoAsync()
    {
        try
        {
            var requestedMode = GetRequestedMode();
            var response = await _apiService.LoginAsDemoAsync(requestedMode);
            if (response == null || response.User.Id == Guid.Empty)
            {
                throw new Exception("Empty demo response");
            }
            _authorizationService.Login(response.User);
            _navigationManager.NavigateTo(UrlService.GetDashboardUrl());
        }
        catch (Exception)
        {
            _isError = true;
            StateHasChanged();
        }
    }
}


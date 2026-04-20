using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class IntegrationsBlock
{
    [Inject]
    public ILogger<IntegrationsBlock> Logger { get; set; } = null!;

    private GetIntegrationSettingsResponse _settings = new();
    private SetClickUpSettingsRequest _clickUpModel = new();
    private SetRedmineSettingsRequest _redmineModel = new();
    private SetJiraSettingsRequest _jiraModel = new();

    private EditForm? _clickUpForm;
    private EditForm? _redmineForm;
    private EditForm? _jiraForm;

    private bool _isLoading = true;
    private IntegrationServiceType? _savingService;
    private IntegrationHelpInfo? _helpInfo;

    private static readonly IntegrationHelpInfo ClickUpHelpInfo = new(
        "ClickUp",
        "Connect a ClickUp workspace by using a personal API token and team ID.",
        "Create or copy a personal API token in ClickUp from the Apps/API section of your ClickUp settings.",
        "Use the ClickUp team ID for the workspace that contains tasks. Enable custom task IDs only when tasks are configured to use them in ClickUp."
    );

    private static readonly IntegrationHelpInfo RedmineHelpInfo = new(
        "Redmine",
        "Connect a Redmine instance by using the site URL, API key, user ID, and time entry activity ID.",
        "Enable REST API in Redmine administration, then copy the API access key from the user account page.",
        "Use the numeric Redmine user ID for the account that owns time entries and the numeric activity ID used for synced time."
    );

    private static readonly IntegrationHelpInfo JiraHelpInfo = new(
        "Jira",
        "Connect a Jira Cloud site by using the site URL, account email, and Atlassian API token.",
        "Create an Atlassian API token from the Atlassian account security page and use the email address for that account.",
        "Use the Jira site URL that contains the target project, for example https://company.atlassian.net."
    );

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        SetWorkspaceIds();

        try
        {
            var settings = await ApiService.WorkspaceIntegrationSettingsGetAsync(AuthState.Value.Workspace!.Id);
            _settings = settings ?? new GetIntegrationSettingsResponse();
            FillModels();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            ToastService.ShowError("Integration settings loading error");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task SaveClickUpAsync()
    {
        if (!IsFormValid(_clickUpForm))
        {
            return;
        }

        await SaveAsync(
            IntegrationServiceType.ClickUp,
            "ClickUp",
            async () =>
            {
                var response = await ApiService.WorkspaceSetClickUpIntegrationSettingsAsync(_clickUpModel);
                _settings.IntegrationClickUp = response;
                if (response != null)
                {
                    _clickUpModel.Fill(response);
                }

                return response?.IsActive == true;
            }
        );
    }

    private async Task SaveRedmineAsync()
    {
        if (!IsFormValid(_redmineForm))
        {
            return;
        }

        await SaveAsync(
            IntegrationServiceType.Redmine,
            "Redmine",
            async () =>
            {
                var response = await ApiService.WorkspaceSetRedmineIntegrationSettingsAsync(_redmineModel);
                _settings.IntegrationRedmine = response;
                if (response != null)
                {
                    _redmineModel.Fill(response);
                }

                return response?.IsActive == true;
            }
        );
    }

    private async Task SaveJiraAsync()
    {
        if (!IsFormValid(_jiraForm))
        {
            return;
        }

        await SaveAsync(
            IntegrationServiceType.Jira,
            "Jira",
            async () =>
            {
                var response = await ApiService.WorkspaceSetJiraIntegrationSettingsAsync(_jiraModel);
                _settings.IntegrationJira = response;
                if (response != null)
                {
                    _jiraModel.Fill(response);
                }

                return response?.IsActive == true;
            }
        );
    }

    private async Task SaveAsync(
        IntegrationServiceType serviceType,
        string serviceName,
        Func<Task<bool>> saveAction
    )
    {
        _savingService = serviceType;
        SetWorkspaceIds();

        try
        {
            var isActive = await saveAction();
            ToastService.ShowSuccess($"{serviceName} settings have been saved");
            if (isActive)
            {
                ToastService.ShowSuccess($"{serviceName} integration is active");
            }
            else
            {
                ToastService.ShowWarning($"{serviceName} integration is inactive. Check the connection settings.");
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            ToastService.ShowError($"{serviceName} settings saving error");
        }
        finally
        {
            _savingService = null;
        }
    }

    private void FillModels()
    {
        if (_settings.IntegrationClickUp != null)
        {
            _clickUpModel.Fill(_settings.IntegrationClickUp);
        }

        if (_settings.IntegrationRedmine != null)
        {
            _redmineModel.Fill(_settings.IntegrationRedmine);
        }

        if (_settings.IntegrationJira != null)
        {
            _jiraModel.Fill(_settings.IntegrationJira);
        }

        SetWorkspaceIds();
    }

    private void SetWorkspaceIds()
    {
        var workspaceId = AuthState.Value.Workspace!.Id;
        _clickUpModel.WorkspaceId = workspaceId;
        _redmineModel.WorkspaceId = workspaceId;
        _jiraModel.WorkspaceId = workspaceId;
    }

    private static bool IsFormValid(EditForm? form)
    {
        return form?.EditContext?.Validate() == true;
    }

    private static bool IsActive(WorkspaceSettingsClickUpDto? settings)
    {
        return settings?.IsActive == true;
    }

    private static bool IsActive(WorkspaceSettingsRedmineDto? settings)
    {
        return settings?.IsActive == true;
    }

    private static bool IsActive(WorkspaceSettingsJiraDto? settings)
    {
        return settings?.IsActive == true;
    }

    private bool IsSaving(IntegrationServiceType serviceType)
    {
        return _savingService == serviceType;
    }

    private void ShowHelp(IntegrationHelpInfo helpInfo)
    {
        _helpInfo = helpInfo;
    }

    private void CloseHelpModal()
    {
        _helpInfo = null;
    }

    private enum IntegrationServiceType
    {
        ClickUp,
        Redmine,
        Jira
    }

    private sealed record IntegrationHelpInfo(
        string Name,
        string Summary,
        string TokenInstruction,
        string FieldsInstruction
    );
}

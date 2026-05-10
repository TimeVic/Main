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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        try
        {
            var settings = await ApiService.WorkspaceIntegrationSettingsGetAsync(AuthState.Value.Workspace!.Id);
            _settings = settings ?? new GetIntegrationSettingsResponse();
            FillModels();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            ToastService.ShowError(DashboardLocalizer["IntegrationsBlock_LoadingError"].Value);
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

        try
        {
            var isActive = await saveAction();
            ToastService.ShowSuccess(string.Format(DashboardLocalizer["IntegrationsBlock_SettingsSaved"].Value, serviceName));
            if (isActive)
            {
                ToastService.ShowSuccess(string.Format(DashboardLocalizer["IntegrationsBlock_IntegrationActive"].Value, serviceName));
            }
            else
            {
                ToastService.ShowWarning(string.Format(DashboardLocalizer["IntegrationsBlock_IntegrationInactive"].Value, serviceName));
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            ToastService.ShowError(string.Format(DashboardLocalizer["IntegrationsBlock_SettingsSavingError"].Value, serviceName));
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

    private IntegrationHelpInfo ClickUpHelpInfo => new(
        "ClickUp",
        DashboardLocalizer["IntegrationsBlock_ClickUpHelpSummary"].Value,
        DashboardLocalizer["IntegrationsBlock_ClickUpHelpTokenInstruction"].Value,
        DashboardLocalizer["IntegrationsBlock_ClickUpHelpFieldsInstruction"].Value
    );

    private IntegrationHelpInfo RedmineHelpInfo => new(
        "Redmine",
        DashboardLocalizer["IntegrationsBlock_RedmineHelpSummary"].Value,
        DashboardLocalizer["IntegrationsBlock_RedmineHelpTokenInstruction"].Value,
        DashboardLocalizer["IntegrationsBlock_RedmineHelpFieldsInstruction"].Value
    );

    private IntegrationHelpInfo JiraHelpInfo => new(
        "Jira",
        DashboardLocalizer["IntegrationsBlock_JiraHelpSummary"].Value,
        DashboardLocalizer["IntegrationsBlock_JiraHelpTokenInstruction"].Value,
        DashboardLocalizer["IntegrationsBlock_JiraHelpFieldsInstruction"].Value
    );

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

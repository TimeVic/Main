using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class AddProjectModal : IDisposable
{
    [CascadingParameter]
    public AppModalInstance ModalInstance { get; set; } = default!;
    
    [Parameter]
    public Guid? InitialClientId { get; set; }

    [Inject]
    public IState<ProjectState> _state { get; set; } = default!;

    [Inject]
    public IState<ClientState> _clientState { get; set; } = default!;

    private AddRequest model = new();
    private EditForm _form = default!;
    
    private bool _isLoading = false;
    private bool _isClientManuallyChanged = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _clientState.StateChanged += OnClientStateChanged;
        InitializeClientId();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        InitializeClientId();
    }

    private void InitializeClientId()
    {
        if (_isClientManuallyChanged)
        {
            return;
        }

        if (InitialClientId.HasValue && InitialClientId.Value != Guid.Empty)
        {
            model.ClientId = InitialClientId.Value;
        }
        else if (model.ClientId == Guid.Empty)
        {
            TrySelectDefaultClient();
        }
    }

    private void OnClientStateChanged(object? sender, EventArgs e)
    {
        if (!_isClientManuallyChanged && model.ClientId == Guid.Empty)
        {
            TrySelectDefaultClient();
        }
        InvokeAsync(StateHasChanged);
    }

    private void TrySelectDefaultClient()
    {
        if (model.ClientId == Guid.Empty && _clientState.Value.List.Any())
        {
            model.ClientId = _clientState.Value.List.First().Id;
        }
    }

    private Task SubmitForm(EditContext editContext)
    {
        return Submit();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        if (model.ClientId == Guid.Empty)
        {
            ToastService.ShowError(DashboardLocalizer["AddProjectModal_ClientRequired"].Value);
            return;
        }

        _isLoading = true;
        StateHasChanged();

        try
        {
            var response = await ApiService.ProjectAddAsync(model);
            if (response != null)
            {
                Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetListItemAction(response));
                ToastService.ShowSuccess(DashboardLocalizer["ProjectAdded"].Value);
                if (ModalInstance != null)
                {
                    await ModalInstance.Close(AppModalResult.Ok(response));
                }
            }
        }
        catch (Exception e)
        {
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void OnClientChanged(ClientDto? client)
    {
        _isClientManuallyChanged = true;
        model.ClientId = client?.Id ?? Guid.Empty;
    }

    public void Dispose()
    {
        _clientState.StateChanged -= OnClientStateChanged;
    }
}

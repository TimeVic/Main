using Fluxor;
using LumexUI.Common;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants.Ui;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Select;

public partial class ProjectsDropDown: IDisposable
{
    [Parameter]
    public InputVariant Variant { get; set; } = InputVariant.Outlined;

    [Parameter]
    public bool ShowProjectsWithoutClients { get; set; } = true;
    
    [Parameter]
    public Guid? ClientId
    {
        get => _clientId;
        set
        {
            if (value != _clientId)
            {
                _clientId = value;
                UpdateList();
            }
        }
    }

    [Inject]
    public IState<ProjectState> _state { get; set; }
    
    private Guid? _clientId;
    private bool _isOpen;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Placeholder = "Select project";

        _state.StateChanged += UpdateList;
        UpdateList();
    }

    private void UpdateList(object? sender, EventArgs e)
    {
        UpdateList();
    }
    
    private void UpdateList()
    {
        _list = _state.Value.List.ToList();
        if (_clientId == Guid.Empty && ShowProjectsWithoutClients)
        {
            _list = _list.Where(item => item.Client == null).ToList();
        }
        else if (_clientId.HasValue && _clientId.Value != Guid.Empty)
        {
            _list = _list.Where(item => item.Client?.Id == _clientId).ToList();
        }
        UpdateSelectedItem();
    }
    
    protected override void UpdateSelectedItem()
    {
        _selectedItem = _list.FirstOrDefault(
            item => item.Id.ToString() == _selectedId
        );
    }

    private Task OnOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        return Task.CompletedTask;
    }

    private async Task OnProjectSelected(ProjectDto? project)
    {
        _isOpen = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        OnValueChanged(project);
    }

    public new void Dispose()
    {
        _state.StateChanged -= UpdateList;
        base.Dispose();
    }
}

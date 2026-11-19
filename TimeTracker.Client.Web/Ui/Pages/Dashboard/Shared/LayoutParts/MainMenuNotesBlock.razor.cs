using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Core.Store.Notes;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainMenuNotesBlock : IDisposable
{
    [Inject]
    private IState<NotesState> NotesState { get; set; } = null!;

    [Parameter]
    public bool IsMobile { get; set; }

    private bool IsSoloWorkspace => AuthState.Value.Workspace?.Mode == WorkspaceMode.Solo;

    private string NotesUrl => UrlService.GetDashboardUrl("notes");

    private string WorkspaceNotesUrl => $"{NotesUrl}?mode=workspace";

    private string PrivateNotesUrl => $"{NotesUrl}?mode=private";

    private bool IsNotesRouteActive => NavigationManager.GetPath()
        .StartsWith(NotesUrl, StringComparison.OrdinalIgnoreCase);

    private bool IsWorkspaceNotesActive => IsNotesRouteActive
        && !IsSoloWorkspace
        && NotesState.Value.ActiveMode == NoteVisibility.Workspace;

    private bool IsPrivateNotesActive => IsNotesRouteActive
        && !IsSoloWorkspace
        && NotesState.Value.ActiveMode == NoteVisibility.Private;

    private string DesktopTriggerClass
    {
        get
        {
            var stateClass = IsNotesRouteActive
                ? "bg-blue-50 text-blue-700"
                : "text-slate-600 hover:bg-slate-100 hover:text-slate-900";

            return $"inline-flex h-10 shrink-0 cursor-pointer items-center gap-2 rounded-lg px-3 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-blue-200 {stateClass}";
        }
    }

    private string MobileLinkClass
    {
        get
        {
            var stateClass = IsNotesRouteActive
                ? "bg-blue-50 text-blue-700"
                : "text-slate-700 hover:bg-slate-100";

            return $"flex w-full items-center gap-3 rounded-lg px-3 py-3 text-base font-medium transition-colors {stateClass}";
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        NavigationManager.LocationChanged += OnLocationChanged;
        NotesState.StateChanged += OnStateChanged;
        AuthState.StateChanged += OnStateChanged;
    }

    private void CreateQuickDraft()
    {
        Dispatcher.Dispatch(new CreateNoteDraftAction(IsNavigateToNotes: true));
    }

    private void OpenPersonalNotes()
    {
        NavigationManager.NavigateTo(NotesUrl);
    }

    private void OpenWorkspaceNotes()
    {
        NavigationManager.NavigateTo(WorkspaceNotesUrl);
    }

    private void OpenPrivateNotes()
    {
        NavigationManager.NavigateTo(PrivateNotesUrl);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnStateChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        NotesState.StateChanged -= OnStateChanged;
        AuthState.StateChanged -= OnStateChanged;
    }
}

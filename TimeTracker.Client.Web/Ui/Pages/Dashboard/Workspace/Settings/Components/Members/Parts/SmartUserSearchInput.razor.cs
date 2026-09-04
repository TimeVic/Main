using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;

public partial class SmartUserSearchInput : IDisposable
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public UserDto? SelectedUser { get; set; }

    [Parameter]
    public EventCallback<UserDto?> SelectedUserChanged { get; set; }

    [Parameter]
    public EventCallback<UserDto> OnUserSelected { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool IsFlat { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    private List<UserDto> _searchResults = new();
    private bool _isOpen;
    private bool _isSearching;
    private bool _isEmailMode;
    private CancellationTokenSource? _debounceCts;

    private async Task OnValueChanged(string input)
    {
        Value = input;
        await ValueChanged.InvokeAsync(Value);

        if (SelectedUser != null && !string.Equals(SelectedUser.Login, input.TrimStart('@'), StringComparison.OrdinalIgnoreCase))
        {
            SelectedUser = null;
            await SelectedUserChanged.InvokeAsync(null);
        }

        var trimmed = input.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex > 0 && atIndex < trimmed.Length - 1)
        {
            // Email mode detected (e.g., name@domain.com)
            _isEmailMode = true;
            _isOpen = false;
            _searchResults.Clear();
            _debounceCts?.Cancel();
            return;
        }

        _isEmailMode = false;
        var cleanQuery = trimmed.TrimStart('@');
        if (cleanQuery.Length >= 2)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            _ = SearchUsersAsync(cleanQuery, token);
        }
        else
        {
            _debounceCts?.Cancel();
            _searchResults.Clear();
            _isOpen = false;
        }
    }

    private async Task SearchUsersAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            _isSearching = true;
            StateHasChanged();

            var response = await ApiService.UserSearchAsync(query);
            if (!token.IsCancellationRequested)
            {
                _searchResults = response?.Items.ToList() ?? [];
                _isOpen = _searchResults.Count > 0;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception)
        {
            _searchResults.Clear();
            _isOpen = false;
        }
        finally
        {
            _isSearching = false;
            StateHasChanged();
        }
    }

    private async Task SelectUser(UserDto user)
    {
        SelectedUser = user;
        Value = $"@{user.Login}";
        _isOpen = false;
        _searchResults.Clear();
        await ValueChanged.InvokeAsync(Value);
        await SelectedUserChanged.InvokeAsync(user);
        await OnUserSelected.InvokeAsync(user);
        StateHasChanged();
    }

    private void OnFocus()
    {
        if (!_isEmailMode && _searchResults.Count > 0)
        {
            _isOpen = true;
        }
    }

    private async Task OnBlur()
    {
        // Delay closing so that click on dropdown item can be registered
        await Task.Delay(200);
        _isOpen = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
    }
}

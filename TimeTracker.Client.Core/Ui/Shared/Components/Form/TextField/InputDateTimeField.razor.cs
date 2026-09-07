using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.TextField;

public partial class InputDateTimeField
{
    [Parameter]
    public bool IsSplit { get; set; } = false;

    [Parameter]
    public Func<DateTime, bool>? DisabledDateFunc { get; set; }

    [Parameter]
    public EventCallback<DateTime?> Changed { get; set; }

    private bool _isOpen;
    private DateTime? _tempDate;
    private string _timeStr = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Value.HasValue && Value.Value != default)
        {
            _tempDate = Value.Value;
            _timeStr = Value.Value.ToString("HH:mm");
        }
        else if (!_isOpen)
        {
            _tempDate = DateTime.Today;
            _timeStr = string.Empty;
        }
    }

    private void TogglePopover()
    {
        if (IsDisabled || IsReadOnly) return;

        _isOpen = !_isOpen;
        if (_isOpen)
        {
            if (Value.HasValue && Value.Value != default)
            {
                _tempDate = Value.Value;
                _timeStr = Value.Value.ToString("HH:mm");
            }
            else
            {
                _tempDate = DateTime.Today;
                _timeStr = DateTime.Now.ToString("HH:mm");
            }
        }
    }

    private void ClosePopover()
    {
        _isOpen = false;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && _isOpen)
        {
            ClosePopover();
        }
    }

    private void OnCalendarDateSelected(DateTime date)
    {
        var h = 0;
        var m = 0;
        if (TryParseTime(_timeStr, out var parsedH, out var parsedM))
        {
            h = parsedH;
            m = parsedM;
        }
        _tempDate = date.Date.Add(new TimeSpan(h, m, 0));
    }

    private void HandleTimeInput(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        _timeStr = raw;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!raw.Contains(':') && digits.Length == 4)
        {
            if (TryParseTime(digits, out var h, out var m))
            {
                _timeStr = $"{h:00}:{m:00}";
            }
        }
    }

    private async Task HandleTimeKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "ArrowUp" or "ArrowDown")
        {
            var isUp = e.Key == "ArrowUp";
            int h = 0, m = 0;
            if (!TryParseTime(_timeStr, out h, out m))
            {
                h = DateTime.Now.Hour;
                m = DateTime.Now.Minute;
            }

            var step = e.ShiftKey ? 15 : 1;
            var totalMin = (h * 60 + m + (isUp ? step : -step)) % 1440;
            if (totalMin < 0) totalMin += 1440;

            _timeStr = $"{totalMin / 60:00}:{totalMin % 60:00}";
        }
        else if (e.Key == "Enter")
        {
            await ConfirmSelection();
        }
    }

    private async Task ConfirmSelection()
    {
        var baseDate = _tempDate?.Date ?? DateTime.Today;
        int h = 0, m = 0;
        TryParseTime(_timeStr, out h, out m);

        var result = baseDate.Add(new TimeSpan(h, m, 0));
        _isOpen = false;

        await SetValueAsync(result);
        await Changed.InvokeAsync(result);
    }

    private async Task HandleClear()
    {
        _tempDate = null;
        _timeStr = string.Empty;
        _isOpen = false;

        await SetValueAsync(null);
        await ClearAsync();
        await Changed.InvokeAsync(null);
    }

    private async Task OnDateOnlyChanged(DateTime? date)
    {
        var time = Value?.TimeOfDay ?? TimeSpan.Zero;
        var result = date.HasValue ? date.Value.Date.Add(time) : (DateTime?)null;
        await SetValueAsync(result);
        await Changed.InvokeAsync(result);
    }

    private async Task OnTimeOnlyChanged(DateTime? time)
    {
        var date = Value?.Date ?? DateTime.Today;
        var result = time.HasValue ? date.Add(time.Value.TimeOfDay) : (DateTime?)null;
        await SetValueAsync(result);
        await Changed.InvokeAsync(result);
    }

    private static bool TryParseTime(string input, out int hours, out int minutes)
    {
        hours = 0;
        minutes = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = input.Trim();
        var parts = trimmed.Split(':');

        if (parts.Length == 2)
        {
            var hPart = new string(parts[0].Where(char.IsDigit).ToArray());
            var mPart = new string(parts[1].Where(char.IsDigit).ToArray());

            if (int.TryParse(hPart, out hours) && int.TryParse(mPart, out minutes))
            {
                if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59)
                {
                    return true;
                }
            }
        }
        else
        {
            var digits = new string(trimmed.Where(char.IsDigit).ToArray());
            if (digits.Length == 4)
            {
                if (int.TryParse(digits.Substring(0, 2), out hours) && int.TryParse(digits.Substring(2, 2), out minutes))
                {
                    if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59)
                    {
                        return true;
                    }
                }
            }
            else if (digits.Length == 3)
            {
                if (int.TryParse(digits.Substring(0, 1), out hours) && int.TryParse(digits.Substring(1, 2), out minutes))
                {
                    if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59)
                    {
                        return true;
                    }
                }
            }
            else if (digits.Length is 1 or 2)
            {
                if (int.TryParse(digits, out hours))
                {
                    if (hours >= 0 && hours <= 23)
                    {
                        minutes = 0;
                        return true;
                    }
                }
            }
        }
        return false;
    }
}

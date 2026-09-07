using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.TextField;

public partial class InputTimeField
{
    [Parameter]
    public TimeSpan? TimeValue { get; set; }

    [Parameter]
    public EventCallback<TimeSpan?> TimeValueChanged { get; set; }

    [Parameter]
    public EventCallback<DateTime?> Changed { get; set; }

    [Parameter]
    public bool IsUpdateOnInput { get; set; }

    private ElementReference _inputRef;
    private string _textValue = string.Empty;
    private DateTime? _baseDate;
    private string lastKeyDownKey = string.Empty;
    private bool _isFocused;

    private bool IsArrowKey(string key) => key is "ArrowUp" or "ArrowDown";

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!_isFocused)
        {
            if (Value.HasValue && Value.Value != default)
            {
                _baseDate = Value.Value.Date;
                _textValue = Value.Value.ToString("HH:mm");
            }
            else if (TimeValue.HasValue)
            {
                _textValue = $"{(int)TimeValue.Value.TotalHours:00}:{TimeValue.Value.Minutes:00}";
            }
            else
            {
                _textValue = string.Empty;
            }
        }
        else if (Value.HasValue && Value.Value != default)
        {
            _baseDate = Value.Value.Date;
        }
    }

    private void HandleFocus()
    {
        _isFocused = true;
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        _textValue = raw;

        // If 4 digits entered without colon (e.g. "1530"), auto-insert colon
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!raw.Contains(':') && digits.Length == 4)
        {
            if (TryParseTime(digits, out var h, out var m))
            {
                _textValue = $"{h:00}:{m:00}";
                if (IsUpdateOnInput)
                {
                    await CommitAsync(h, m);
                }
                return;
            }
        }

        // If valid HH:mm with 2-digit minute is entered, commit immediately
        if (raw.Contains(':'))
        {
            var parts = raw.Split(':');
            if (parts.Length == 2 && parts[1].Length == 2)
            {
                if (TryParseTime(raw, out var h, out var m))
                {
                    if (IsUpdateOnInput)
                    {
                        await CommitAsync(h, m);
                    }
                }
            }
        }
    }

    private async Task HandleBlur()
    {
        _isFocused = false;

        if (string.IsNullOrWhiteSpace(_textValue))
        {
            if (Value.HasValue || TimeValue.HasValue)
            {
                await ClearTimeAsync();
            }
            return;
        }

        if (TryParseTime(_textValue, out var hours, out var minutes))
        {
            _textValue = $"{hours:00}:{minutes:00}";
            await CommitAsync(hours, minutes);
        }
        else if (Value.HasValue && Value.Value != default)
        {
            _textValue = Value.Value.ToString("HH:mm");
        }
        else if (TimeValue.HasValue)
        {
            _textValue = $"{(int)TimeValue.Value.TotalHours:00}:{TimeValue.Value.Minutes:00}";
        }
        else
        {
            _textValue = string.Empty;
            if (Value.HasValue || TimeValue.HasValue)
            {
                await ClearTimeAsync();
            }
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        lastKeyDownKey = e.Key;

        if (e.Key is "ArrowUp" or "ArrowDown")
        {
            var isUp = e.Key == "ArrowUp";
            var caretPos = 0;

            try
            {
                caretPos = await JSRuntime.InvokeAsync<int>("getTimeCaret", _inputRef);
            }
            catch
            {
                caretPos = 4;
            }

            int h = 0, m = 0;
            if (!TryParseTime(_textValue, out h, out m))
            {
                h = DateTime.Now.Hour;
                m = DateTime.Now.Minute;
            }

            if (caretPos <= 2)
            {
                h = isUp ? (h + 1) % 24 : (h + 23) % 24;
            }
            else
            {
                var step = e.ShiftKey ? 15 : 1;
                m = isUp ? (m + step) % 60 : (m + 60 - (step % 60)) % 60;
            }

            _textValue = $"{h:00}:{m:00}";
            await CommitAsync(h, m);
            StateHasChanged();

            try
            {
                await JSRuntime.InvokeVoidAsync("setTimeCaret", _inputRef, caretPos);
            }
            catch {}
        }
        else if (e.Key == "Enter")
        {
            await HandleBlur();
        }
    }

    private async Task StepMinutes(int step)
    {
        if (IsDisabled || IsReadOnly) return;

        int h = 0, m = 0;
        if (!TryParseTime(_textValue, out h, out m))
        {
            h = DateTime.Now.Hour;
            m = DateTime.Now.Minute;
        }

        var totalMinutes = (h * 60 + m + step) % 1440;
        if (totalMinutes < 0) totalMinutes += 1440;

        h = totalMinutes / 60;
        m = totalMinutes % 60;

        _textValue = $"{h:00}:{m:00}";
        await CommitAsync(h, m);
    }

    private async Task CommitAsync(int hours, int minutes)
    {
        hours = Math.Clamp(hours, 0, 23);
        minutes = Math.Clamp(minutes, 0, 59);

        var ts = new TimeSpan(hours, minutes, 0);
        var baseDate = _baseDate ?? DateTime.Today;
        var dt = baseDate.Date.Add(ts);

        // Do not commit or trigger events if the time value has not changed
        if (Value.HasValue && Value.Value != default)
        {
            if (Value.Value.Date == dt.Date && Value.Value.Hour == hours && Value.Value.Minute == minutes)
            {
                return;
            }
        }
        else if (TimeValue.HasValue)
        {
            if ((int)TimeValue.Value.TotalHours == hours && TimeValue.Value.Minutes == minutes)
            {
                return;
            }
        }

        await SetValueAsync(dt);
        await TimeValueChanged.InvokeAsync(ts);
        await Changed.InvokeAsync(dt);
    }

    public async Task ClearTimeAsync()
    {
        _textValue = string.Empty;
        if (!Value.HasValue && !TimeValue.HasValue)
        {
            return;
        }

        await SetValueAsync(null);
        await TimeValueChanged.InvokeAsync(null);
        await Changed.InvokeAsync(null);
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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using TimeTracker.Client.Core.Constants.Ui;
using TimeTracker.Client.Core.Core.Components;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

public abstract class AppBaseSelect : BaseReactiveComponent, IDisposable
{
    [CascadingParameter]
    protected EditContext? CurrentEditContext { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public SelectSize Size { get; set; } = SelectSize.Medium;

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool Disabled
    {
        get => IsDisabled;
        set => IsDisabled = value;
    }

    [Parameter]
    public bool IsClearable { get; set; }

    [Parameter]
    public bool Clearable
    {
        get => IsClearable;
        set => IsClearable = value;
    }

    [Parameter]
    public bool IsFullWidth { get; set; }

    [Parameter]
    public bool FullWidth
    {
        get => IsFullWidth;
        set => IsFullWidth = value;
    }

    [Parameter]
    public bool IsMultiple { get; set; }

    [Parameter]
    public SelectVariant Variant { get; set; } = SelectVariant.Input;

    [Parameter]
    public DropDownType? SelectType { get; set; }

    [Parameter]
    public SelectColor Color { get; set; } = SelectColor.Default;

    [Parameter]
    public SelectColor DropDownColor
    {
        get => Color;
        set => Color = value;
    }

    [Parameter]
    public string? ButtonColorClass { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? PopupClass { get; set; }

    public bool IsOpen { get; protected set; }

    protected virtual string ButtonColorClasses
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ButtonColorClass))
            {
                return ButtonColorClass;
            }

            return Color switch
            {
                SelectColor.Primary => "bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-800 hover:bg-blue-100",
                SelectColor.Success => "bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800 hover:bg-emerald-100",
                SelectColor.Warning => "bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 border-amber-200 dark:border-amber-800 hover:bg-amber-100",
                SelectColor.Danger => "bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300 border-rose-200 dark:border-rose-800 hover:bg-rose-100",
                SelectColor.Secondary => "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 border-slate-200 dark:border-slate-700 hover:bg-slate-200",
                SelectColor.Info => "bg-sky-50 dark:bg-sky-950/40 text-sky-700 dark:text-sky-300 border-sky-200 dark:border-sky-800 hover:bg-sky-100",
                _ => "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-200 border-slate-200 dark:border-slate-700 hover:bg-slate-200"
            };
        }
    }

    protected FieldIdentifier FieldIdentifier;

    protected bool IsInvalid => CurrentEditContext != null
                                && FieldIdentifier.Model != null
                                && CurrentEditContext.GetValidationMessages(FieldIdentifier).Any();

    protected SelectVariant ResolvedVariant => SelectType switch
    {
        DropDownType.DropDown => SelectVariant.Button,
        DropDownType.Select => SelectVariant.Input,
        _ => Variant
    };

    public virtual void ToggleOpen()
    {
        if (IsDisabled)
        {
            return;
        }

        IsOpen = !IsOpen;
        StateHasChanged();
    }

    public virtual void Open()
    {
        if (IsDisabled || IsOpen)
        {
            return;
        }

        IsOpen = true;
        StateHasChanged();
    }

    public virtual void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        StateHasChanged();
    }

    protected virtual void HandleKeyDown(KeyboardEventArgs e)
    {
        if (IsDisabled)
        {
            return;
        }

        if (e.Key == "Escape" && IsOpen)
        {
            Close();
        }
        else if ((e.Key == "Enter" || e.Key == " " || e.Key == "ArrowDown") && !IsOpen)
        {
            Open();
        }
    }

    protected virtual void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        StateHasChanged();
    }

    public virtual void Dispose()
    {
        if (CurrentEditContext != null)
        {
            CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}

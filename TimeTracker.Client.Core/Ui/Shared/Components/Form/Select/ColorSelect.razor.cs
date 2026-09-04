using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Constants.Ui;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form.Select;

public partial class ColorSelect
{
    private sealed record TagColorOption(string Code);

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public DropDownType Type { get; set; } = DropDownType.DropDown;

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool FullWidth { get; set; } = true;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    private static readonly IReadOnlyList<string> ColorCodes =
    [
        "#EF4444", "#F97316", "#F59E0B", "#EAB308", "#84CC16", "#22C55E",
        "#10B981", "#14B8A6", "#06B6D4", "#0EA5E9", "#3B82F6", "#6366F1",
        "#8B5CF6", "#A855F7", "#D946EF", "#EC4899", "#F43F5E", "#64748B",
        "#475569", "#334155", "#1F2937", "#111827", "#7C3AED", "#2563EB",
        "#0891B2", "#0F766E", "#16A34A", "#65A30D", "#CA8A04", "#D97706",
        "#DC2626", "#BE123C"
    ];

    protected SelectVariant ResolvedVariant => Type switch
    {
        DropDownType.DropDown => SelectVariant.Button,
        _ => SelectVariant.Input
    };

    private bool HasError => EditContext?.GetValidationMessages(FieldIdentifier).Any() ?? false;

    private string FirstError =>
        EditContext?.GetValidationMessages(FieldIdentifier).FirstOrDefault() ?? string.Empty;

    private string LocalizedPlaceholder =>
        string.IsNullOrWhiteSpace(Placeholder) ? DashboardLocalizer["SelectColor"].Value : Placeholder;

    private void OnColorSelected(string? color)
    {
        CurrentValue = string.IsNullOrEmpty(color) ? null : color;
    }

    private static string GetColorStyle(string color)
    {
        return $"background-color: {color};";
    }

    protected override bool TryParseValueFromString(
        string? value,
        out string? result,
        out string validationErrorMessage
    )
    {
        result = value;
        validationErrorMessage = string.Empty;
        return true;
    }
}

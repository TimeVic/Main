using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Core.Helpers;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Form;

public partial class MarkdownEdit
{
    private enum MarkdownFormat
    {
        Heading1,
        Heading2,
        Heading3,
        Bold,
        Italic,
        Strikethrough,
        Quote,
        Code,
        Link,
        BulletedList,
        OrderedList,
        TaskList
    }

    private enum MarkdownEditorMode
    {
        Edit,
        View
    }

    private sealed record MarkdownFormatResult(string Value, int CaretPosition);

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public string TextAreaName { get; set; } = string.Empty;

    [Parameter]
    public int MaxLength { get; set; } = 10000;

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public string EmptyText { get; set; } = string.Empty;

    [Parameter]
    public int RowsMin { get; set; } = 6;

    [Parameter]
    public int MinHeight { get; set; } = 180;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<string> Changed { get; set; }

    [Parameter]
    public EventCallback OnClickedToView { get; set; }

    [Parameter]
    public bool IsEditMode { get; set; }

    [Parameter]
    public bool IsInlineActionsEnabled { get; set; } = true;

    [Parameter]
    public bool IsModeTabsVisible { get; set; } = true;

    [Inject]
    public UiHelperService UiHelperService { get; set; } = null!;

    private ElementReference _editor;
    private const int EditorToolbarHeight = 47;
    private const int EditorSurfaceBorderHeight = 2;
    private bool _isShowEditBox;
    private MarkdownEditorMode _editorMode = MarkdownEditorMode.Edit;
    private readonly int _textAreaRowsMax = 20;
    private int _textAreaRows = 3;
    private string _value = string.Empty;
    private string _editValue = string.Empty;
    private string _lastParameterValue = string.Empty;
    private string _lastSubmittedValue = string.Empty;
    private MarkdownFormatResult? _pendingMarkdownFormat;
    private bool _isActionButtonsEnabled;
    private bool _isInitialized;
    private bool _isAwaitingSubmittedValue;

    private int PreviewMinHeight => MinHeight + EditorToolbarHeight + EditorSurfaceBorderHeight;

    protected override void OnParametersSet()
    {
        if (!IsModeTabsVisible)
        {
            _editorMode = MarkdownEditorMode.Edit;
        }

        var value = Value ?? string.Empty;
        if (_isInitialized && _isAwaitingSubmittedValue)
        {
            if (value == _lastSubmittedValue)
            {
                _isAwaitingSubmittedValue = false;
            }
            else if (value == _lastParameterValue)
            {
                return;
            }
            else
            {
                _isAwaitingSubmittedValue = false;
            }
        }

        var isDesiredEditBox = IsEditMode;
        _lastParameterValue = value;
        if (_isInitialized && (IsInlineActionsEnabled && _isActionButtonsEnabled || _value == value && _isShowEditBox == isDesiredEditBox))
        {
            return;
        }

        _value = value;
        _editValue = value;
        _isShowEditBox = isDesiredEditBox;
        _isActionButtonsEnabled = false;
        _isInitialized = true;
        ResizeEditBox(_editValue);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingMarkdownFormat == null)
        {
            return;
        }

        var format = _pendingMarkdownFormat;
        _pendingMarkdownFormat = null;
        await UiHelperService.SetTextAreaSelection(_editor, format.CaretPosition, format.CaretPosition);
    }

    private void ResizeEditBox(string? description)
    {
        description ??= string.Empty;
        _textAreaRows = Math.Max(description.Split('\n').Length, description.Split('\r').Length);
        _textAreaRows = Math.Max(_textAreaRows, RowsMin);
        _textAreaRows = Math.Min(_textAreaRows, _textAreaRowsMax);
    }

    private async Task OnEditValueChanged()
    {
        ResizeEditBox(_editValue);

        if (!IsInlineActionsEnabled)
        {
            _value = _editValue;
            TrackSubmittedValue(_value);
            await ValueChanged.InvokeAsync(_value);
            await Changed.InvokeAsync(_value);
            return;
        }

        _isActionButtonsEnabled = _isActionButtonsEnabled || _editValue != _value;
    }

    private async Task OnClickView()
    {
        if (await UiHelperService.IsTextSelected())
        {
            return;
        }

        _isShowEditBox = true;
        _isActionButtonsEnabled = IsInlineActionsEnabled;
        StateHasChanged();
    }

    private void SetEditorMode(MarkdownEditorMode editorMode)
    {
        _editorMode = editorMode;
    }

    private string GetEditorTabClass(MarkdownEditorMode editorMode)
    {
        return editorMode == _editorMode
            ? "w-markdown-editor-tab is-active"
            : "w-markdown-editor-tab";
    }

    private async Task ApplyMarkdownFormatAsync(MarkdownFormat format)
    {
        var editorState = await UiHelperService.GetTextAreaState(_editor);
        var result = ApplyMarkdownFormat(editorState, format);
        if (MaxLength > 0 && result.Value.Length > MaxLength)
        {
            return;
        }

        _editValue = result.Value;
        _pendingMarkdownFormat = result;
        await OnEditValueChanged();
    }

    private static MarkdownFormatResult ApplyMarkdownFormat(TextAreaState editorState, MarkdownFormat format)
    {
        return format switch
        {
            MarkdownFormat.Heading1 => ApplyLineFormat(editorState, _ => "# "),
            MarkdownFormat.Heading2 => ApplyLineFormat(editorState, _ => "## "),
            MarkdownFormat.Heading3 => ApplyLineFormat(editorState, _ => "### "),
            MarkdownFormat.Bold => ApplyInlineFormat(editorState, "**", "**"),
            MarkdownFormat.Italic => ApplyInlineFormat(editorState, "*", "*"),
            MarkdownFormat.Strikethrough => ApplyInlineFormat(editorState, "~~", "~~"),
            MarkdownFormat.Quote => ApplyLineFormat(editorState, _ => "> "),
            MarkdownFormat.Code => ApplyInlineFormat(editorState, "`", "`"),
            MarkdownFormat.Link => ApplyInlineFormat(editorState, "[", "]()"),
            MarkdownFormat.BulletedList => ApplyLineFormat(editorState, _ => "- "),
            MarkdownFormat.OrderedList => ApplyLineFormat(editorState, index => $"{index + 1}. "),
            MarkdownFormat.TaskList => ApplyLineFormat(editorState, _ => "- [ ] "),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static MarkdownFormatResult ApplyInlineFormat(TextAreaState editorState, string prefix, string suffix)
    {
        var selectedText = editorState.Value[editorState.SelectionStart..editorState.SelectionEnd];
        var value = string.Concat(
            editorState.Value[..editorState.SelectionStart],
            prefix,
            selectedText,
            suffix,
            editorState.Value[editorState.SelectionEnd..]);
        var caretPosition = editorState.SelectionStart + prefix.Length + selectedText.Length;

        return new MarkdownFormatResult(value, caretPosition);
    }

    private static MarkdownFormatResult ApplyLineFormat(TextAreaState editorState, Func<int, string> prefixFactory)
    {
        var lineStart = editorState.Value.LastIndexOf('\n', Math.Max(0, editorState.SelectionStart - 1)) + 1;
        var nextLineBreak = editorState.Value.IndexOf('\n', editorState.SelectionEnd);
        var lineEnd = nextLineBreak == -1 ? editorState.Value.Length : nextLineBreak;
        var lines = editorState.Value[lineStart..lineEnd].Split('\n');
        var formattedText = string.Join('\n', lines.Select((line, index) => $"{prefixFactory(index)}{line}"));
        var value = string.Concat(
            editorState.Value[..lineStart],
            formattedText,
            editorState.Value[lineEnd..]);
        var caretPosition = editorState.SelectionStart == editorState.SelectionEnd
            ? editorState.SelectionStart + prefixFactory(0).Length
            : lineStart + formattedText.Length;

        return new MarkdownFormatResult(value, caretPosition);
    }

    private async Task OnClickSave()
    {
        await SaveEditValue();
    }

    private async Task SaveEditValue()
    {
        var isChanged = _value != _editValue;
        _value = _editValue;
        _isShowEditBox = IsEditMode;
        _isActionButtonsEnabled = false;
        if (isChanged)
        {
            TrackSubmittedValue(_value);
            await ValueChanged.InvokeAsync(_value);
            await Changed.InvokeAsync(_value);
        }
    }

    private void OnClickCancel()
    {
        _isShowEditBox = IsEditMode;
        _editorMode = MarkdownEditorMode.Edit;
        _editValue = _value;
        _isActionButtonsEnabled = false;
        ResizeEditBox(_editValue);
    }

    private void TrackSubmittedValue(string value)
    {
        _lastSubmittedValue = value;
        _isAwaitingSubmittedValue = true;
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TimeTracker.Client.Web.Ui.Shared.Components.Layout;

public partial class ResizableSplitBlock
{
    private const int KeyboardResizeStep = 16;

    private bool _isInitialized;
    private bool _isResizing;
    private double _resizeStartClientX;
    private int _resizeStartPaneWidth;
    private int _startPaneWidth;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool IsContentOverflowVisible { get; set; }

    [Parameter]
    public int DefaultStartPaneWidth { get; set; } = 320;

    [Parameter]
    public int MinStartPaneWidth { get; set; } = 240;

    [Parameter]
    public int MaxStartPaneWidth { get; set; } = 560;

    [Parameter]
    public string HandleLabel { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public RenderFragment StartContent { get; set; } = null!;

    [Parameter, EditorRequired]
    public RenderFragment EndContent { get; set; } = null!;

    private string ContainerClass
    {
        get
        {
            var baseClass = _isResizing
                ? "resizable-split-block is-resizing"
                : "resizable-split-block";

            if (IsContentOverflowVisible)
            {
                baseClass += " is-content-overflow-visible";
            }

            return string.IsNullOrWhiteSpace(Class)
                ? baseClass
                : $"{baseClass} {Class}";
        }
    }

    private string StyleAttribute =>
        $"--resizable-split-start-width: {_startPaneWidth}px; " +
        $"--resizable-split-start-min-width: {MinStartPaneWidth}px; " +
        $"--resizable-split-start-max-width: {MaxStartPaneWidth}px;";

    protected override void OnParametersSet()
    {
        if (!_isInitialized)
        {
            _startPaneWidth = ClampWidth(DefaultStartPaneWidth);
            _isInitialized = true;
            return;
        }

        _startPaneWidth = ClampWidth(_startPaneWidth);
    }

    private void OnResizePointerDown(PointerEventArgs args)
    {
        if (args.Button != 0)
        {
            return;
        }

        _isResizing = true;
        _resizeStartClientX = args.ClientX;
        _resizeStartPaneWidth = _startPaneWidth;
    }

    private void OnResizePointerMove(PointerEventArgs args)
    {
        if (!_isResizing)
        {
            return;
        }

        var delta = args.ClientX - _resizeStartClientX;
        _startPaneWidth = ClampWidth((int)Math.Round(_resizeStartPaneWidth + delta));
    }

    private void OnResizePointerUp()
    {
        _isResizing = false;
    }

    private void OnResizeHandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "ArrowLeft")
        {
            _startPaneWidth = ClampWidth(_startPaneWidth - KeyboardResizeStep);
        }
        else if (args.Key == "ArrowRight")
        {
            _startPaneWidth = ClampWidth(_startPaneWidth + KeyboardResizeStep);
        }
        else if (args.Key == "Home")
        {
            _startPaneWidth = ClampWidth(MinStartPaneWidth);
        }
        else if (args.Key == "End")
        {
            _startPaneWidth = ClampWidth(MaxStartPaneWidth);
        }
    }

    private int ClampWidth(int width)
    {
        return Math.Min(Math.Max(width, MinStartPaneWidth), MaxStartPaneWidth);
    }
}

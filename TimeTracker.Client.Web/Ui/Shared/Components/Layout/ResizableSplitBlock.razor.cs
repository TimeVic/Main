using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TimeTracker.Client.Web.Ui.Shared.Components.Layout;

public partial class ResizableSplitBlock : IAsyncDisposable
{
    private const string JsModulePath = "./js/resizableSplitBlock.js";

    private ElementReference _rootElement;
    private IJSObjectReference? _module;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public int DefaultStartPaneWidth { get; set; } = 320;

    [Parameter]
    public int MinStartPaneWidth { get; set; } = 240;

    [Parameter]
    public int MaxStartPaneWidth { get; set; } = 560;

    [Parameter]
    public string DesktopMediaQuery { get; set; } = "(min-width: 1280px)";

    [Parameter]
    public string HandleLabel { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public RenderFragment StartContent { get; set; } = null!;

    [Parameter, EditorRequired]
    public RenderFragment EndContent { get; set; } = null!;

    private string ContainerClass => string.IsNullOrWhiteSpace(Class)
        ? "resizable-split-block"
        : $"resizable-split-block {Class}";

    private string StyleAttribute =>
        $"--resizable-split-start-width: {DefaultStartPaneWidth}px; " +
        $"--resizable-split-start-min-width: {MinStartPaneWidth}px; " +
        $"--resizable-split-start-max-width: {MaxStartPaneWidth}px;";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _module = await Js.InvokeAsync<IJSObjectReference>("import", JsModulePath);
        await _module.InvokeVoidAsync("initialize", _rootElement, new
        {
            defaultStartPaneWidth = DefaultStartPaneWidth,
            minStartPaneWidth = MinStartPaneWidth,
            maxStartPaneWidth = MaxStartPaneWidth,
            desktopMediaQuery = DesktopMediaQuery
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_module == null)
        {
            return;
        }

        await _module.InvokeVoidAsync("dispose", _rootElement);
        await _module.DisposeAsync();
    }
}

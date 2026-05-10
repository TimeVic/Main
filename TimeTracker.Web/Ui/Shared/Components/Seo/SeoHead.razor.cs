using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Web.Services;

namespace TimeTracker.Web.Ui.Shared.Components.Seo;

public partial class SeoHead
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Inject]
    private ISeoUrlService SeoUrlService { get; set; } = null!;

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Keywords { get; set; }

    [Parameter]
    public string? Robots { get; set; }

    [Parameter]
    public string? CanonicalUrl { get; set; }

    [Parameter]
    public string? PublicPagePath { get; set; }

    protected string? ResolvedCanonicalUrl { get; private set; }

    protected IReadOnlyCollection<SeoAlternateUrl> AlternateUrls { get; private set; } = [];

    private string DocumentLanguage { get; set; } = ILocalizationUrlService.EnglishCultureName;

    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(PublicPagePath))
        {
            ResolvedCanonicalUrl = CanonicalUrl;
            AlternateUrls = [];
            DocumentLanguage = ILocalizationUrlService.EnglishCultureName;
            return;
        }

        var currentPath = new Uri(NavigationManager.Uri).AbsolutePath;
        var metadata = SeoUrlService.GetPublicPageMetadata(PublicPagePath, currentPath);
        ResolvedCanonicalUrl = metadata.CanonicalUrl;
        AlternateUrls = metadata.AlternateUrls;
        DocumentLanguage = metadata.DocumentLanguage;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Js.InvokeVoidAsync("setDocumentLanguage", DocumentLanguage);
    }
}

using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Shared.Components.Helper.Portal;

public class PopupPortalStore
{
    public static string? ActivePopupId { get; private set; }
    public static RenderFragment? Content { get; private set; }
    public static string PanelStyle { get; private set; } = string.Empty;

    public static event Action? OnChange;

    public static bool IsOpen(string popupId) => ActivePopupId == popupId;

    public static void Show(string popupId, RenderFragment content, string panelStyle)
    {
        ActivePopupId = popupId;
        Content = content;
        PanelStyle = panelStyle;
        OnChange?.Invoke();
    }

    public static void Close()
    {
        ActivePopupId = null;
        Content = null;
        PanelStyle = string.Empty;
        OnChange?.Invoke();
    }
}

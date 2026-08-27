using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Web.Ui.Pages.Landing.Shared;

public partial class MainHeaderBlock
{
    [Inject]
    protected IState<AuthState> AuthState { get; set; }

    [Inject]
    protected ILocalizationUrlService LocalizationUrlService { get; set; }

    protected string CurrentPath => NavigationManager.GetPath();

    protected string CurrentCulture => LocalizationUrlService.GetCurrentCultureName(CurrentPath);

    protected bool IsUkrainianLocale => CurrentCulture == ILocalizationUrlService.UkrainianCultureName;

    protected string NavHomeUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Main, CurrentCulture);

    protected string NavReportsAndPayoutsUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.FeaturesReportsAndPayouts, CurrentCulture);

    protected string NavTimeAndApprovalsUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.FeaturesTimeAndApprovals, CurrentCulture);

    protected string NavTasksAndNotesUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.FeaturesTasksAndNotes, CurrentCulture);

    protected string NavUseCasesUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.UseCases, CurrentCulture);

    protected string NavPricingUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Pricing, CurrentCulture);

    protected string NavFaqUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Faq, CurrentCulture);

    protected string RegistrationUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Registration_Step1, CurrentCulture);

    protected string LoginUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Login, CurrentCulture);

    protected string EnglishUrl => LocalizationUrlService.GetEnglishUrl(CurrentPath);

    protected string UkrainianUrl => LocalizationUrlService.GetUkrainianUrl(CurrentPath);

    protected Task SwitchToEnglish() => SwitchLanguageAsync(ILocalizationUrlService.EnglishCultureName, EnglishUrl);

    protected Task SwitchToUkrainian() => SwitchLanguageAsync(ILocalizationUrlService.UkrainianCultureName, UkrainianUrl);

    private async Task SwitchLanguageAsync(string cultureName, string targetUrl)
    {
        if (AuthState.Value.IsLoggedIn)
        {
            var user = await ApiService.UserUpdateSettingsAsync(new UpdateSettingsRequest
            {
                UserName = AuthState.Value.User?.UserName,
                LanguageCode = cultureName
            });
            if (user != null)
            {
                Dispatcher.Dispatch(new UpdateUserAction(user));
            }
        }

        await Js.InvokeVoidAsync("localStorage.setItem", ILocalizationUrlService.LocalStorageKey, cultureName);
        NavigationManager.NavigateTo(targetUrl, forceLoad: true);
    }
}

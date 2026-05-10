using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Services;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Landing.Shared;

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

    protected string NavUseCasesUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.UseCases, CurrentCulture);

    protected string NavPricingUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Pricing, CurrentCulture);

    protected string NavFaqUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Faq, CurrentCulture);

    protected string RegistrationUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Registration_Step1, CurrentCulture);

    protected string LoginUrl => LocalizationUrlService.GetLocalizedUrl(SiteUrl.Login, CurrentCulture);

    protected string EnglishUrl => LocalizationUrlService.GetEnglishUrl(CurrentPath);

    protected string UkrainianUrl => LocalizationUrlService.GetUkrainianUrl(CurrentPath);

    protected void SwitchToEnglish() => NavigationManager.NavigateTo(EnglishUrl, forceLoad: true);

    protected void SwitchToUkrainian() => NavigationManager.NavigateTo(UkrainianUrl, forceLoad: true);
}

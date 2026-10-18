using Microsoft.Extensions.Logging;
using VieDon.App.Pages;
using VieDon.App.Services;
using VieDon.App.ViewModels;

namespace VieDon.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>(); // polices du système : aucune police embarquée (écoconception)

        // Services (InMemoryAuthService = développement seulement)
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IAuthService, InMemoryAuthService>();
        builder.Services.AddSingleton<IAlertService, InMemoryAlertService>();

        // Écrans et modèles de vue
        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<WelcomeViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DonorSignupPage>();
        builder.Services.AddTransient<DonorSignupViewModel>();
        builder.Services.AddTransient<DonorHomePage>();
        builder.Services.AddTransient<DonorHomeViewModel>();
        builder.Services.AddTransient<AlertDetailPage>();
        builder.Services.AddTransient<AlertDetailViewModel>();
        builder.Services.AddTransient<ParticipationPage>();
        builder.Services.AddTransient<ParticipationViewModel>();
        builder.Services.AddTransient<RequesterSignupPage>();
        builder.Services.AddTransient<RequesterSignupViewModel>();
        builder.Services.AddTransient<RequesterVerificationPage>();
        builder.Services.AddTransient<RequesterVerificationViewModel>();
        builder.Services.AddTransient<RequesterHomePage>();
        builder.Services.AddTransient<RequesterHomeViewModel>();
        builder.Services.AddTransient<NewAlertPage>();
        builder.Services.AddTransient<NewAlertViewModel>();
        builder.Services.AddTransient<PlaceholderPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}

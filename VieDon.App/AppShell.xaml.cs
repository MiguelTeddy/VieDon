using VieDon.App.Pages;
using VieDon.App.Services;

namespace VieDon.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(Routes.Login, typeof(LoginPage));
        Routing.RegisterRoute(Routes.DonorSignup, typeof(DonorSignupPage));
        Routing.RegisterRoute(Routes.AlertDetail, typeof(AlertDetailPage));
        Routing.RegisterRoute(Routes.Participation, typeof(ParticipationPage));
        Routing.RegisterRoute(Routes.RequesterSignup, typeof(RequesterSignupPage));
        Routing.RegisterRoute(Routes.RequesterVerification, typeof(RequesterVerificationPage));
        Routing.RegisterRoute(Routes.NewAlert, typeof(NewAlertPage));
    }
}

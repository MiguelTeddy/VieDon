using VieDon.App.Localization;

namespace VieDon.App.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task ShowMessageAsync(string title, string message);
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task ShowMessageAsync(string title, string message) =>
        Shell.Current.DisplayAlert(Loc.T(title), Loc.T(message), "OK");

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        Shell.Current.DisplayAlert(Loc.T(title), Loc.T(message), Loc.T(accept), Loc.T(cancel));
}

public static class Routes
{
    public const string Login = "login";
    public const string DonorSignup = "donorsignup";
    public const string AlertDetail = "alert";
    public const string Participation = "participation";
    public const string RequesterSignup = "requestersignup";
    public const string RequesterVerification = "requesterverification";
    public const string NewAlert = "newalert";
    public const string RewardDetail = "reward";
    public const string ValidateDonation = "validatedonation";
    public const string Welcome = "//welcome";
    public const string Admin = "//admin";
    public const string DonorHome = "//donor";
    public const string RequesterHome = "//requester";
}

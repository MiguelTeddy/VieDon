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
        Shell.Current.DisplayAlert(title, message, "OK");

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        Shell.Current.DisplayAlert(title, message, accept, cancel);
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
    public const string Welcome = "//welcome";
    public const string DonorHome = "//donor";
    public const string RequesterHome = "//requester";
}

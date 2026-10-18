using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 21 : Accueil demandeur (individu simple, établissement avec indicateurs).</summary>
public partial class RequesterHomeViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IAlertService _alerts;
    private readonly INavigationService _nav;

    public RequesterHomeViewModel(ISessionService session, IAlertService alerts, INavigationService nav)
    {
        _session = session;
        _alerts = alerts;
        _nav = nav;
    }

    private UserProfile? User => _session.CurrentUser;

    public ObservableCollection<RequesterAlertItemViewModel> Alerts { get; } = new();

    public string Greeting => User is null ? "Bonjour" : $"Bonjour, {User.DisplayName}";
    public bool IsEstablishment => User?.Kind == RequesterKind.Establishment;
    public bool ShowVerifiedBadge => IsEstablishment && User?.Verification == VerificationStatus.Verified;
    public bool ShowPendingBanner => User?.Verification == VerificationStatus.Pending;
    public bool ShowRejectedBanner => User?.Verification == VerificationStatus.Rejected;
    public bool CanCreate => User is not null && AlertPolicy.CanLaunch(User);

    [ObservableProperty] private string requestsText = "0";
    [ObservableProperty] private string filledText = "0";
    [ObservableProperty] private string successText = "-";

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showEmpty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    public void Refresh() => OnPropertyChanged(string.Empty);

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (User is null) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        ShowEmpty = false;
        try
        {
            var items = await _alerts.GetRequesterAlertsAsync(User);
            Alerts.Clear();
            foreach (var s in items) Alerts.Add(new RequesterAlertItemViewModel(s, Open));
            ShowEmpty = Alerts.Count == 0;

            // Indicateurs calculés à partir des alertes réelles de la session (aucun chiffre inventé).
            var total = items.Count;
            var filled = items.Count(i => i.Accepted >= i.Alert.Pouches);
            RequestsText = total.ToString();
            FilledText = filled.ToString();
            SuccessText = total == 0 ? "-" : $"{filled * 100 / total} %";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger tes alertes. Vérifie ta connexion.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally { IsLoading = false; }
    }

    private void Open(RequesterAlertItemViewModel item) =>
        _ = _nav.ShowMessageAsync("Suivi de l'alerte", "L'écran 23 (suivi d'une alerte) sera ajouté dans la prochaine étape.");

    [RelayCommand]
    private async Task NewAlertAsync()
    {
        if (!CanCreate)
        {
            await _nav.ShowMessageAsync("Compte non vérifié",
                "Tu pourras lancer une alerte dès que ton compte sera vérifié.");
            return;
        }
        await _nav.GoToAsync(Routes.NewAlert);
    }

    [RelayCommand]
    private async Task OpenVerificationAsync() => await _nav.GoToAsync(Routes.RequesterVerification);

    [RelayCommand]
    private async Task ValidateDonationAsync() =>
        await _nav.ShowMessageAsync("Valider un don", "L'écran 24 (valider un don) sera ajouté dans la prochaine étape.");

    // Provisoire, en attendant l'écran 27 (Profil demandeur).
    [RelayCommand]
    private async Task LogoutAsync()
    {
        _session.Clear();
        await _nav.GoToAsync(Routes.Welcome);
    }
}

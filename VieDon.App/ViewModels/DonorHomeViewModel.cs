using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Écran 7 : Accueil donneur (éligibilité, alertes proches, groupe à confirmer).</summary>
public partial class DonorHomeViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IAlertService _alerts;
    private readonly INavigationService _nav;

    public DonorHomeViewModel(ISessionService session, IAlertService alerts, INavigationService nav)
    {
        _session = session;
        _alerts = alerts;
        _nav = nav;
    }

    private UserProfile? User => _session.CurrentUser;

    public ObservableCollection<AlertItemViewModel> Alerts { get; } = new();

    public string Greeting => User is null ? "Bonjour" : $"Bonjour, {User.FirstName}";
    public string GroupSummary => User is null ? string.Empty : $"Groupe {User.GroupLabel} - {User.GroupStatusLabel}";

    // RG8 : statut du groupe toujours visible ; bandeau tant que le groupe n'est pas confirmé.
    public bool ShowConfirmBanner => User is not null && User.GroupStatus == GroupStatus.Declared;
    public bool ShowUnknownGroupHint => User is not null && User.GroupStatus == GroupStatus.Unknown;

    // TODO : règle d'éligibilité à valider auprès d'un centre de transfusion (RG12). Affichage indicatif.
    public string EligibilityText => "Tu peux donner";
    public string EligibilityHint => "Prochain don possible : maintenant. Indication seulement : la décision finale revient au centre de don.";
    public string LevelText => "Niveau : aucun don validé pour l'instant";
    public double LevelProgress => 0;

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
            var alerts = await _alerts.GetNearbyAlertsAsync(User);
            Alerts.Clear();
            foreach (var a in alerts) Alerts.Add(new AlertItemViewModel(a, Open));
            ShowEmpty = Alerts.Count == 0 && !ShowUnknownGroupHint;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les alertes. Vérifie ta connexion.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Open(AlertItemViewModel item) =>
        _ = _nav.GoToAsync($"{Routes.AlertDetail}?id={item.Alert.Id}");

    [RelayCommand]
    private async Task ConfirmGroupAsync() =>
        await _nav.ShowMessageAsync("Confirmer mon groupe", "L'écran 12 du cahier sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task ChangeZoneAsync() =>
        await _nav.ShowMessageAsync("Zone et géolocalisation", "L'écran 14 du cahier sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task FindCentresAsync() =>
        await _nav.ShowMessageAsync("Centres de collecte", "L'écran 13 du cahier sera ajouté dans une prochaine étape.");

    // Provisoire, en attendant l'écran 18 (Profil) : permet de tester plusieurs comptes.
    [RelayCommand]
    private async Task LogoutAsync()
    {
        _session.Clear();
        await _nav.GoToAsync(Routes.Welcome);
    }
}

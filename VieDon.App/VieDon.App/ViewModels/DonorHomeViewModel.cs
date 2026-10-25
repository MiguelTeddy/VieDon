using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 7 : Accueil donneur (éligibilité, niveau, alertes proches, groupe à confirmer).</summary>
public partial class DonorHomeViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IAlertService _alerts;
    private readonly IDonationService _donations;
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;

    public DonorHomeViewModel(ISessionService session, IAlertService alerts, IDonationService donations,
        IAuthService auth, INavigationService nav)
    {
        _session = session;
        _alerts = alerts;
        _donations = donations;
        _auth = auth;
        _nav = nav;
    }

    private UserProfile? User => _session.CurrentUser;

    public ObservableCollection<AlertItemViewModel> Alerts { get; } = new();

    public string Greeting => User is null ? "Bonjour" : $"Bonjour, {User.FirstName}";
    public string Initials => UiAssets.Initials(User?.FullName);
    [ObservableProperty] private string medalImage = "medal_none.png";

    public string GroupSummary => User is null ? string.Empty : $"Groupe {User.GroupLabel} - {User.GroupStatusLabel}";

    // RG8 : statut du groupe toujours visible ; bandeau tant que le groupe n'est pas confirmé.
    public bool ShowConfirmBanner => User is not null && User.GroupStatus == GroupStatus.Declared;
    public bool ShowUnknownGroupHint => User is not null && User.GroupStatus == GroupStatus.Unknown;
    public bool ShowUnavailableBanner => User is not null && !User.IsAvailable;
    public bool ShowGroupLine => User is not null && User.GroupStatus == GroupStatus.Confirmed;

    // TODO : règle d'éligibilité à valider auprès d'un centre de transfusion (RG12). Affichage indicatif.
    [ObservableProperty] private bool canDonate = true;
    [ObservableProperty] private string eligibilityText = "✓ Tu peux donner";
    [ObservableProperty] private string eligibilityHint = "Prochain don possible : maintenant. Indication seulement : la décision finale revient au centre de don.";
    [ObservableProperty] private string levelText = "Niveau : aucun don validé pour l'instant";
    [ObservableProperty] private double levelProgress;
    [ObservableProperty] private string pendingText = string.Empty;
    [ObservableProperty] private bool hasPending;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showEmpty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    public void Refresh() => OnPropertyChanged(string.Empty);

    private void ApplyStats(DonationStats stats)
    {
        CanDonate = stats.CanDonateNow;
        const string indicative = "Indication seulement : la décision finale revient au centre de don.";
        if (stats.CanDonateNow)
        {
            EligibilityText = "✓ Tu peux donner";
            EligibilityHint = $"Prochain don possible : maintenant. {indicative}";
        }
        else
        {
            var next = stats.NextPossible!.Value.Date;
            var days = Math.Max(1, (next - DateTime.Today).Days);
            EligibilityText = "▲ Pas encore";
            EligibilityHint = $"Prochain don possible le {Labels.DateText(next)} (dans {Labels.DaysText(days)}). {indicative}";
        }

        LevelText = stats.Level == DonorLevel.None
            ? "Niveau : aucun don validé pour l'instant"
            : $"Niveau {LevelPolicy.Label(stats.Level)} - {Labels.DonationsText(stats.ValidatedCount)}";
        LevelProgress = LevelPolicy.Progress(stats.ValidatedCount);
        MedalImage = UiAssets.Medal(stats.Level);

        HasPending = stats.PendingCount > 0;
        PendingText = stats.PendingCount == 1
            ? "▲ 1 don en attente de validation par le centre."
            : $"▲ {stats.PendingCount} dons en attente de validation par le centre.";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (User is null) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        ShowEmpty = false;
        try
        {
            // Relit le profil : le groupe a pu être confirmé par un centre depuis la dernière visite (RG8).
            var fresh = await _auth.GetProfileAsync(User.Phone);
            if (fresh is not null) _session.CurrentUser = fresh;
            OnPropertyChanged(string.Empty);

            var donations = await _donations.GetDonationsAsync(User!.Phone);
            ApplyStats(DonationStatsCalculator.Compute(donations, DateTime.Today));

            var alerts = await _alerts.GetNearbyAlertsAsync(User);
            Alerts.Clear();
            foreach (var a in alerts) Alerts.Add(new AlertItemViewModel(a, Open));
            ShowEmpty = Alerts.Count == 0 && !ShowUnknownGroupHint && !ShowUnavailableBanner;
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
        await _nav.ShowMessageAsync("Confirmer mon groupe",
            "Ton groupe est confirmé quand un centre valide ton don, ou sur justificatif vérifié, ou après un test en centre. L'écran 12 complet sera ajouté plus tard.");

    [RelayCommand]
    private async Task ChangeZoneAsync() =>
        await _nav.ShowMessageAsync("Zone et géolocalisation", "L'écran 14 du cahier sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task FindCentresAsync() =>
        await _nav.ShowMessageAsync("Centres de collecte", "L'écran 13 du cahier sera ajouté dans une prochaine étape.");

    // Le bouton Se déconnecter est maintenant dans l'onglet Profil.
    [RelayCommand]
    private async Task LogoutAsync()
    {
        _session.Clear();
        await _nav.GoToAsync(Routes.Welcome);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 18 : Profil donneur (informations, disponibilité, liens, changement d'espace, déconnexion).</summary>
public partial class DonorProfileViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IAuthService _auth;
    private readonly IDonationService _donations;
    private readonly INavigationService _nav;
    private bool _loading;

    public DonorProfileViewModel(ISessionService session, IAuthService auth, IDonationService donations, INavigationService nav)
    {
        _session = session;
        _auth = auth;
        _donations = donations;
        _nav = nav;
    }

    private UserProfile? User => _session.CurrentUser;

    public string FullName => User?.FullName ?? string.Empty;
    public string PhoneText => User is null ? string.Empty : $"Téléphone : {User.Phone}";
    public string GroupLine => User is null ? string.Empty : $"Groupe {User.GroupLabel} - {User.GroupStatusLabel}";
    public string ZoneText => User is null ? string.Empty : $"Zone : {User.City}";
    public string PrivacyText => "Tes coordonnées restent masquées : les échanges passent par VieDon. Aucun nom ni diagnostic n'est partagé.";

    public string Initials => UiAssets.Initials(User?.FullName);
    [ObservableProperty] private string medalImage = "medal_none.png";

    [ObservableProperty] private string presencesText = string.Empty;
    [ObservableProperty] private string levelText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailabilityText))]
    private bool isAvailable = true;

    public string AvailabilityText => IsAvailable
        ? "✓ Disponible : tu reçois les alertes de ton groupe."
        : "▲ Indisponible : tu ne reçois plus d'alertes.";

    [ObservableProperty] private bool isSaving;

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
        try
        {
            var fresh = await _auth.GetProfileAsync(User.Phone);
            if (fresh is not null) _session.CurrentUser = fresh;

            var donations = await _donations.GetDonationsAsync(User!.Phone);
            var stats = DonationStatsCalculator.Compute(donations, DateTime.Today);
            PresencesText = $"Présences confirmées : {stats.PresencesCount}";
            MedalImage = UiAssets.Medal(stats.Level);
            LevelText = stats.Level == DonorLevel.None
                ? "Niveau : aucun pour l'instant"
                : $"Niveau : {LevelPolicy.Label(stats.Level)} ({stats.ValidatedCount} dons validés)";

            _loading = true;
            IsAvailable = User!.IsAvailable;
            _loading = false;
            OnPropertyChanged(string.Empty);
        }
        catch (Exception)
        {
            _loading = false;
            ErrorMessage = "Impossible de charger ton profil.";
        }
    }

    partial void OnIsAvailableChanged(bool value)
    {
        if (_loading || User is null) return;
        _ = SaveAvailabilityAsync(value);
    }

    private async Task SaveAvailabilityAsync(bool value)
    {
        IsSaving = true;
        ErrorMessage = string.Empty;
        try
        {
            await _auth.SetAvailabilityAsync(User!.Phone, value);
            var fresh = await _auth.GetProfileAsync(User.Phone);
            if (fresh is not null) _session.CurrentUser = fresh;
            SemanticScreenReader.Announce(AvailabilityText);
        }
        catch (Exception)
        {
            ErrorMessage = "Enregistrement impossible. Réessaie.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ConfirmGroupAsync() =>
        await _nav.ShowMessageAsync("Confirmer mon groupe",
            "Ton groupe est confirmé quand un centre valide ton don, sur justificatif vérifié, ou après un test en centre. L'écran 12 complet sera ajouté plus tard.");

    [RelayCommand]
    private async Task ZoneAsync() =>
        await _nav.ShowMessageAsync("Zone et géolocalisation", "L'écran 14 du cahier sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task NotificationsAsync() =>
        await _nav.ShowMessageAsync("Notifications et rappels", "L'écran 15 du cahier sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task HelpAsync() =>
        await _nav.ShowMessageAsync("Aide, conditions et confidentialité", "L'écran 5 du cahier sera ajouté dans une prochaine étape.");

    // RG24 : un seul compte, un espace actif à la fois (activation depuis le profil, à valider).
    [RelayCommand]
    private async Task SwitchSpaceAsync()
    {
        var yes = await _nav.ConfirmAsync("Passer à l'espace Demandeur ?",
            "Tu seras déconnecté. Tu pourras te connecter avec un compte demandeur ou en créer un. Un compte n'a qu'un espace actif à la fois.",
            "Continuer", "Annuler");
        if (!yes) return;
        _session.Clear();
        _session.SelectedSpace = UserSpace.Requester;
        await _nav.GoToAsync(Routes.Welcome);
        await _nav.GoToAsync(Routes.Login);
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _session.Clear();
        await _nav.GoToAsync(Routes.Welcome);
    }

    // RG23 : suppression du compte et des données en deux gestes (le bouton, puis la confirmation).
    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        if (User is null) return;
        var yes = await _nav.ConfirmAsync("Supprimer mon compte ?",
            "Ton compte et tes données seront supprimés. Cette action est définitive.", "Supprimer", "Garder");
        if (!yes) return;
        await _auth.DeleteAccountAsync(User.Phone);
        _session.Clear();
        await _nav.GoToAsync(Routes.Welcome);
    }
}

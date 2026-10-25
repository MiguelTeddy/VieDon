using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Écran 27 : Profil demandeur (informations, vérification, abonnement, changement d'espace, déconnexion).</summary>
public partial class RequesterProfileViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IAuthService _auth;
    private readonly IAlertService _alerts;
    private readonly INavigationService _nav;

    public RequesterProfileViewModel(ISessionService session, IAuthService auth, IAlertService alerts, INavigationService nav)
    {
        _session = session;
        _auth = auth;
        _alerts = alerts;
        _nav = nav;
    }

    private UserProfile? User => _session.CurrentUser;

    public string DisplayName => User?.DisplayName ?? string.Empty;
    public bool IsEstablishment => User?.Kind == RequesterKind.Establishment;
    public bool IsIndividual => !IsEstablishment;

    public string KindText => IsEstablishment ? "Établissement de santé" : "Individu (proche d'un patient)";
    public string OrgTypeText => User?.OrganizationType is EstablishmentType t ? $"Type : {Labels.EstablishmentTypeLabel(t)}" : string.Empty;
    public bool HasOrgType => User?.OrganizationType is not null;
    public string AddressText => User is null || string.IsNullOrWhiteSpace(User.City) ? string.Empty : $"Adresse : {User.City}";
    public string ResponsibleText => User is null ? string.Empty : IsEstablishment ? $"Responsable : {User.FullName}" : $"Nom : {User.FullName}";
    public string PhoneText => User is null ? string.Empty : $"Téléphone : {User.Phone}";

    // Le statut de vérification se dit en symbole + mot (jamais la couleur seule).
    public bool IsVerified => User?.Verification == VerificationStatus.Verified;
    public bool IsPending => User?.Verification == VerificationStatus.Pending;
    public bool IsRejected => User?.Verification == VerificationStatus.Rejected;

    [ObservableProperty] private string statsText = string.Empty;

    public void Refresh() => OnPropertyChanged(string.Empty);

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (User is null) return;
        try
        {
            var fresh = await _auth.GetProfileAsync(User.Phone);
            if (fresh is not null) _session.CurrentUser = fresh;
            OnPropertyChanged(string.Empty);

            var items = await _alerts.GetRequesterAlertsAsync(User!);
            var filled = items.Count(i => i.Alert.Status == AlertStatus.Filled);
            StatsText = items.Count == 0
                ? "Aucune alerte lancée pour l'instant."
                : $"{items.Count} alertes lancées, dont {filled} pourvues.";
        }
        catch (Exception)
        {
            StatsText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task OpenVerificationAsync() => await _nav.GoToAsync(Routes.RequesterVerification);

    [RelayCommand]
    private async Task SubscriptionAsync() =>
        await _nav.ShowMessageAsync("Abonnement et facturation",
            "Plan actuel : essai gratuit. Les plans (Essai gratuit, Standard, Premium) et leurs tarifs seront précisés après le pilote. L'écran 26 sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task HelpAsync() =>
        await _nav.ShowMessageAsync("Aide, conditions et confidentialité", "L'écran 5 du cahier sera ajouté dans une prochaine étape.");

    // RG24 : un seul compte, un espace actif à la fois (activation depuis le profil, à valider).
    [RelayCommand]
    private async Task SwitchSpaceAsync()
    {
        var yes = await _nav.ConfirmAsync("Passer à l'espace Donneur ?",
            "Tu seras déconnecté. Tu pourras te connecter avec un compte donneur ou en créer un. Un compte n'a qu'un espace actif à la fois.",
            "Continuer", "Annuler");
        if (!yes) return;
        _session.Clear();
        _session.SelectedSpace = UserSpace.Donor;
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

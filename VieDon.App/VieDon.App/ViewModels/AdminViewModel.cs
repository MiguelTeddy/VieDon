using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Administration de démonstration : liste de tous les comptes avec leurs dons et alertes,
/// et décision sur les établissements en attente de vérification (RG15 : normalement faite par une personne).
/// </summary>
public partial class AdminViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IDonationService _donations;
    private readonly IAlertService _alerts;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public AdminViewModel(IAuthService auth, IDonationService donations, IAlertService alerts,
        ISessionService session, INavigationService nav)
    {
        _auth = auth;
        _donations = donations;
        _alerts = alerts;
        _session = session;
        _nav = nav;
    }

    public ObservableCollection<AdminAccountItem> Donors { get; } = new();
    public ObservableCollection<AdminAccountItem> Requesters { get; } = new();

    [ObservableProperty] private string summaryText = string.Empty;
    [ObservableProperty] private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    public string NoteText =>
        "Compte d'administration de développement : il n'existe pas dans la version finale. Les données sont fictives et en mémoire.";

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var profiles = await _auth.GetAllProfilesAsync();
            Donors.Clear();
            Requesters.Clear();

            foreach (var p in profiles.Where(p => p.Space == UserSpace.Donor))
            {
                var donations = await _donations.GetDonationsAsync(p.Phone);
                var stats = DonationStatsCalculator.Compute(donations, DateTime.Today);
                var level = stats.Level == DonorLevel.None ? "aucun niveau" : $"niveau {LevelPolicy.Label(stats.Level)}";
                Donors.Add(new AdminAccountItem(
                    p.FullName,
                    $"Groupe {p.GroupLabel} - {p.GroupStatusLabel}",
                    $"{Labels.DonationsText(stats.ValidatedCount)} - {level} - {stats.PresencesCount} présences",
                    $"{(p.IsAvailable ? "✓ Disponible" : "▲ Indisponible")} - Téléphone {p.Phone}",
                    string.Empty, false, false, DecideAsync, p.Phone));
            }

            foreach (var p in profiles.Where(p => p.Space == UserSpace.Requester))
            {
                var alerts = await _alerts.GetRequesterAlertsAsync(p);
                var filled = alerts.Count(a => a.Alert.Status == AlertStatus.Filled);
                var active = alerts.Count(a => a.Alert.Status == AlertStatus.Active);
                var kind = p.Kind == RequesterKind.Establishment
                    ? (p.OrganizationType is EstablishmentType t ? $"Établissement : {Labels.EstablishmentTypeLabel(t)}" : "Établissement")
                    : "Individu";
                var verification = p.Verification switch
                {
                    VerificationStatus.Verified => "✓ Compte vérifié",
                    VerificationStatus.Rejected => "✕ Vérification refusée",
                    _ => "▲ En attente de vérification"
                };
                Requesters.Add(new AdminAccountItem(
                    p.DisplayName,
                    kind,
                    $"{alerts.Count} alertes : {filled} pourvues, {active} en cours",
                    $"Responsable {p.FullName} - Téléphone {p.Phone}",
                    verification,
                    p.Kind == RequesterKind.Establishment && p.Verification == VerificationStatus.Pending,
                    true, DecideAsync, p.Phone));
            }

            var pending = Requesters.Count(r => r.CanDecide);
            SummaryText = $"{Donors.Count} donneurs, {Requesters.Count} demandeurs, {pending} vérification(s) en attente.";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger les comptes.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DecideAsync(AdminAccountItem item, bool approve)
    {
        await _auth.SimulateVerificationAsync(item.Phone, approve);
        SemanticScreenReader.Announce(approve ? $"{item.Title} : compte validé." : $"{item.Title} : compte refusé.");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _session.Clear();
        await _nav.GoToAsync(Routes.Welcome);
    }
}

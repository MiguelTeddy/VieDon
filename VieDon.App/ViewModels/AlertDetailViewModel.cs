using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Écran 8 : Alerte reçue. La notification ouvre directement cet écran ; deux actions suffisent.</summary>
public partial class AlertDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAlertService _alerts;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;
    private Guid _id;

    public AlertDetailViewModel(IAlertService alerts, ISessionService session, INavigationService nav)
    {
        _alerts = alerts;
        _session = session;
        _nav = nav;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && Guid.TryParse(value?.ToString(), out var id))
        {
            _id = id;
            _ = LoadAsync();
        }
    }

    // ----- Contenu de l'alerte (RG18 : ni nom ni diagnostic du patient)
    [ObservableProperty] private bool hasAlert;
    [ObservableProperty] private bool isCritical;
    [ObservableProperty] private bool isUrgent;
    [ObservableProperty] private bool isNormal;
    [ObservableProperty] private string urgencyLabel = string.Empty;
    [ObservableProperty] private string groupText = string.Empty;
    [ObservableProperty] private string pouchesText = string.Empty;
    [ObservableProperty] private string establishmentName = string.Empty;
    [ObservableProperty] private bool isVerified;
    [ObservableProperty] private bool isNotVerified;
    [ObservableProperty] private string distanceText = string.Empty;
    [ObservableProperty] private string ageText = string.Empty;
    [ObservableProperty] private string zoneText = string.Empty;

    // ----- États
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isInactive;
    [ObservableProperty] private bool isWaiting;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AcceptCommand))] private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    public bool ShowActions => HasAlert && !IsInactive && !IsWaiting;
    public string InactiveText => "Cette alerte n'est plus active. Merci d'avoir répondu présent.";
    public string WaitingText => "Quota atteint : tu seras prévenu si besoin.";
    public string ReminderText => "Ton groupe sera contrôlé à ton arrivée au centre.";

    partial void OnHasAlertChanged(bool value) => OnPropertyChanged(nameof(ShowActions));
    partial void OnIsInactiveChanged(bool value) => OnPropertyChanged(nameof(ShowActions));
    partial void OnIsWaitingChanged(bool value) => OnPropertyChanged(nameof(ShowActions));

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var alert = await _alerts.GetAlertAsync(_id);
            if (alert is null) { IsInactive = true; return; }

            IsCritical = alert.Level == UrgencyLevel.Critical;
            IsUrgent = alert.Level == UrgencyLevel.Urgent;
            IsNormal = alert.Level == UrgencyLevel.Normal;
            UrgencyLabel = Labels.UrgencyLabel(alert.Level);
            GroupText = Labels.GroupLabel(alert.Group, alert.RhesusFactor);
            PouchesText = Labels.PouchesText(alert.Pouches);
            EstablishmentName = alert.Establishment.Name;
            IsVerified = alert.Establishment.IsVerified && !alert.IsFromIndividual;
            IsNotVerified = !IsVerified;
            DistanceText = $"{Labels.DistanceText(alert.DistanceKm)} de toi";
            AgeText = $"Demande {Labels.Ago(alert.CreatedAt, DateTimeOffset.Now)}";
            ZoneText = $"Zone : {alert.Zone}";
            IsInactive = alert.Status != AlertStatus.Active; // pourvue, annulée ou expirée
            HasAlert = true;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger l'alerte. Vérifie ta connexion.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAccept() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private async Task AcceptAsync()
    {
        if (_session.CurrentUser is null) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _alerts.AcceptAsync(_id, _session.CurrentUser);
            switch (result.Status)
            {
                case AcceptStatus.Confirmed:
                    await _nav.GoToAsync($"{Routes.Participation}?alertId={_id}");
                    break;
                case AcceptStatus.WaitingList:
                    IsWaiting = true;
                    SemanticScreenReader.Announce(WaitingText);
                    break;
                default:
                    IsInactive = true;
                    SemanticScreenReader.Announce(InactiveText);
                    break;
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Connexion interrompue. Réessaie dans un instant.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeclineAsync() => await _nav.GoToAsync("..");

    [RelayCommand]
    private async Task BackHomeAsync() => await _nav.GoToAsync("..");

    [RelayCommand]
    private async Task ShowEstablishmentAsync() =>
        await _nav.ShowMessageAsync(EstablishmentName,
            IsVerified
                ? "Établissement vérifié : son justificatif d'autorisation a été contrôlé par une personne. La fiche complète (alertes passées et pourvues) sera ajoutée plus tard."
                : "Alerte non vérifiée : elle a été lancée par un particulier. Le rendez-vous a toujours lieu dans un établissement de santé.");

    [RelayCommand]
    private async Task ReportAsync() =>
        await _nav.ShowMessageAsync("Signaler", "L'écran 4 du cahier (signalement) sera ajouté dans une prochaine étape.");
}

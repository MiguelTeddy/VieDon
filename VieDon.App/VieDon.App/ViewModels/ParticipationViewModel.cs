using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;

namespace VieDon.App.ViewModels;

/// <summary>Écran 9 : Confirmation et suivi du don (code à présenter au centre).</summary>
public partial class ParticipationViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAlertService _alerts;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;
    private Guid _alertId;

    public ParticipationViewModel(IAlertService alerts, ISessionService session, INavigationService nav)
    {
        _alerts = alerts;
        _session = session;
        _nav = nav;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("alertId", out var value) && Guid.TryParse(value?.ToString(), out var id))
        {
            _alertId = id;
            _ = LoadAsync();
        }
    }

    [ObservableProperty] private bool isLoaded;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private string establishmentName = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string deadlineText = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;

    // RG16 : les coordonnées sont masquées ; le contact passe par VieDon.
    public string ContactText => "Contact masqué : les échanges passent par VieDon.";
    public string CodeHint => "À présenter sur place. QR code à ajouter dans une prochaine étape.";

    private async Task LoadAsync()
    {
        if (_session.CurrentUser is null) return;
        var p = await _alerts.GetParticipationAsync(_alertId, _session.CurrentUser);
        if (p is null)
        {
            ErrorMessage = "Participation introuvable.";
            return;
        }
        Code = p.Code;
        EstablishmentName = p.EstablishmentName;
        Address = p.Address;
        DeadlineText = $"Aujourd'hui, avant {p.Deadline.ToLocalTime():HH:mm}";
        IsLoaded = true;
    }

    [RelayCommand]
    private async Task DirectionsAsync()
    {
        // Itinéraire via l'application de cartes (adresse envoyée uniquement à la demande du donneur).
        var uri = new Uri($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString($"{EstablishmentName} {Address}")}");
        await Launcher.Default.OpenAsync(uri);
    }

    [RelayCommand]
    private async Task CancelParticipationAsync()
    {
        var yes = await _nav.ConfirmAsync("Annuler ma participation ?",
            "Sans pénalité si tu préviens à l'avance.", "Annuler ma participation", "Garder");
        if (!yes) return;
        if (_session.CurrentUser is not null)
            await _alerts.CancelParticipationAsync(_alertId, _session.CurrentUser);
        await _nav.GoToAsync(Routes.DonorHome);
    }

    [RelayCommand]
    private async Task BackHomeAsync() => await _nav.GoToAsync(Routes.DonorHome);
}

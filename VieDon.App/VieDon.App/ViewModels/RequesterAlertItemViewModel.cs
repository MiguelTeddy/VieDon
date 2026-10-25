using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Carte d'une alerte lancée par le demandeur (écran 21).</summary>
public sealed class RequesterAlertItemViewModel
{
    public RequesterAlertItemViewModel(RequesterAlertSummary summary, Action<RequesterAlertItemViewModel> open)
    {
        Summary = summary;
        OpenCommand = new RelayCommand(() => open(this));
    }

    public RequesterAlertSummary Summary { get; }
    public Alert Alert => Summary.Alert;
    public IRelayCommand OpenCommand { get; }

    public bool IsCritical => Alert.Level == UrgencyLevel.Critical;
    public bool IsUrgent => Alert.Level == UrgencyLevel.Urgent;
    public bool IsNormal => Alert.Level == UrgencyLevel.Normal;

    public string UrgencyLabel => Labels.UrgencyLabel(Alert.Level);
    public string GroupLine => $"{Labels.GroupLabel(Alert.Group, Alert.RhesusFactor)} - {Labels.PouchesText(Alert.Pouches)}";
    public string PlaceName => Alert.Establishment.Name;
    public string CountersText => $"{Summary.Contacted} contactés - {Summary.Accepted} acceptés";
    public string AgeText => Labels.Ago(Alert.CreatedAt, DateTimeOffset.Now);
    public bool IsActive => Alert.Status == AlertStatus.Active;
    public bool IsFilled => Alert.Status == AlertStatus.Filled || Summary.Accepted >= Alert.Pouches;
    public string DateText => Labels.DateText(Alert.CreatedAt.LocalDateTime);

    // Le statut se dit en mot et en symbole, jamais par la couleur seule.
    public string StatusText => Alert.Status switch
    {
        AlertStatus.Filled => "✓ Pourvue",
        AlertStatus.Expired => "▲ Expirée",
        AlertStatus.Cancelled => "✕ Annulée",
        _ => IsFilled ? "✓ Besoin couvert" : "Recherche en cours"
    };
    public string ButtonDescription => $"Suivre l'alerte {UrgencyLabel}, {GroupLine}, {CountersText}";
}

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
    public bool IsFilled => Summary.Accepted >= Alert.Pouches;
    public string StatusText => IsFilled ? "Besoin couvert" : "Recherche en cours";
    public string ButtonDescription => $"Suivre l'alerte {UrgencyLabel}, {GroupLine}, {CountersText}";
}

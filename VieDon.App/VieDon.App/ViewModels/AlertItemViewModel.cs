using CommunityToolkit.Mvvm.Input;
using VieDon.Core.Formatting;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Carte d'alerte de l'Accueil donneur.</summary>
public sealed class AlertItemViewModel
{
    public AlertItemViewModel(Alert alert, Action<AlertItemViewModel> open)
    {
        Alert = alert;
        OpenCommand = new RelayCommand(() => open(this));
    }

    public Alert Alert { get; }
    public IRelayCommand OpenCommand { get; }

    public bool IsCritical => Alert.Level == UrgencyLevel.Critical;
    public bool IsUrgent => Alert.Level == UrgencyLevel.Urgent;
    public bool IsNormal => Alert.Level == UrgencyLevel.Normal;

    public string UrgencyLabel => Labels.UrgencyLabel(Alert.Level);
    public string GroupText => Labels.GroupLabel(Alert.Group, Alert.RhesusFactor);
    public string GroupLine => $"Groupe recherché : {GroupText}";
    public string EstablishmentName => Alert.Establishment.Name;

    // Une alerte d'individu reste « Non vérifiée » (RG15).
    public bool IsVerified => Alert.Establishment.IsVerified && !Alert.IsFromIndividual;
    public bool IsNotVerified => !IsVerified;

    public string DistanceText => Labels.DistanceText(Alert.DistanceKm);
    public string PouchesText => Labels.PouchesText(Alert.Pouches);
    public string InfoLine => $"{DistanceText} - {PouchesText}";
    public string AgeText => Labels.Ago(Alert.CreatedAt, DateTimeOffset.Now);

    public string ButtonDescription =>
        $"Voir et répondre. Alerte {UrgencyLabel}. {GroupLine}. {EstablishmentName}, {(IsVerified ? "vérifié" : "non vérifié")}. {InfoLine}.";
}

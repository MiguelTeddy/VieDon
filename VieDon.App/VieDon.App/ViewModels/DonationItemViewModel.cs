using VieDon.Core.Formatting;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Ligne de l'historique de dons (écran 11). Le statut se dit en mot et en symbole.</summary>
public sealed class DonationItemViewModel
{
    public DonationItemViewModel(Donation donation) => Donation = donation;

    public Donation Donation { get; }

    public string DateText => Labels.DateText(Donation.Date);
    public string PlaceText => $"{Donation.PlaceName}, {Donation.City}";

    public bool IsValidated => Donation.Status == DonationStatus.Validated;
    public bool IsPending => Donation.Status == DonationStatus.Pending;
    public bool IsAttended => Donation.Status == DonationStatus.AttendedNoDonation;

    public string StatusText => Donation.Status switch
    {
        DonationStatus.Validated => "✓ Don validé",
        DonationStatus.Pending => "▲ En attente de validation",
        _ => "i Présent, don non réalisé"
    };

    public string Description => $"{DateText}. {PlaceText}. {StatusText}.";
}

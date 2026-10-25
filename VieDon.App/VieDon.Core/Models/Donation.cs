namespace VieDon.Core.Models;

/// <summary>Validated = don validé par un centre (seul statut comptabilisé, RG10) ; Pending = accepté, pas encore validé ;
/// AttendedNoDonation = présent au centre mais don non réalisé (compte dans les présences confirmées).</summary>
public enum DonationStatus { Validated, Pending, AttendedNoDonation }

public sealed record Donation(
    Guid Id,
    DateTime Date,
    string PlaceName,
    string City,
    DonationStatus Status,
    Guid? AlertId = null);

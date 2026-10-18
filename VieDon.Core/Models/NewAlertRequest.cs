namespace VieDon.Core.Models;

/// <summary>
/// Données saisies pour lancer une alerte. RG18 : ni nom ni diagnostic du patient.
/// RG17 : le lieu est toujours un établissement de santé ou un centre de collecte.
/// </summary>
public sealed record NewAlertRequest(
    BloodGroup Group,
    Rhesus RhesusFactor,
    int Pouches,
    string PlaceName,
    UrgencyLevel Level,
    int RadiusKm,
    bool IncludeCompatible);

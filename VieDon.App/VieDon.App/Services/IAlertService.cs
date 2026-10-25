using VieDon.Core.Models;

namespace VieDon.App.Services;

public enum AcceptStatus { Confirmed, WaitingList, NoLongerActive }
public sealed record AcceptResult(AcceptStatus Status, Participation? Participation = null);

public enum ValidationStatus { Validated, UnknownCode, AlreadyValidated, NotAllowed }
public sealed record DonationValidation(
    ValidationStatus Status, string? DonorPhone = null, string? DonorName = null, string? GroupLabel = null, string? PlaceName = null);

public sealed record RequesterAlertSummary(Alert Alert, int Contacted, int Accepted);

/// <summary>Contrat des alertes (côtés donneur et demandeur). L'implémentation réelle appellera l'API VieDon.</summary>
public interface IAlertService
{
    Task<IReadOnlyList<Alert>> GetNearbyAlertsAsync(UserProfile donor, CancellationToken ct = default);
    Task<Alert?> GetAlertAsync(Guid id, CancellationToken ct = default);
    Task<AcceptResult> AcceptAsync(Guid alertId, UserProfile donor, CancellationToken ct = default);
    Task<Participation?> GetParticipationAsync(Guid alertId, UserProfile donor, CancellationToken ct = default);
    Task CancelParticipationAsync(Guid alertId, UserProfile donor, CancellationToken ct = default);

    // Côté demandeur
    Task<IReadOnlyList<RequesterAlertSummary>> GetRequesterAlertsAsync(UserProfile requester, CancellationToken ct = default);
    Task<Alert> CreateAlertAsync(NewAlertRequest request, UserProfile requester, CancellationToken ct = default);

    /// <summary>Écran 24 : un établissement vérifié valide un don avec le code présenté par le donneur (RG10).</summary>
    Task<DonationValidation> ValidateDonationAsync(string code, UserProfile validator, CancellationToken ct = default);
}

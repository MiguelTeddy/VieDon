using VieDon.Core.Models;

namespace VieDon.App.Services;

public enum AcceptStatus { Confirmed, WaitingList, NoLongerActive }
public sealed record AcceptResult(AcceptStatus Status, Participation? Participation = null);

public sealed record RequesterAlertSummary(Alert Alert, int Contacted, int Accepted);

/// <summary>Contrat des alertes (côtés donneur et demandeur). L'implémentation réelle appellera l'API VieDon.</summary>
public interface IAlertService
{
    Task<IReadOnlyList<Alert>> GetNearbyAlertsAsync(UserProfile donor, CancellationToken ct = default);
    Task<Alert?> GetAlertAsync(Guid id, CancellationToken ct = default);
    Task<AcceptResult> AcceptAsync(Guid alertId, UserProfile donor, CancellationToken ct = default);
    Task<Participation?> GetParticipationAsync(Guid alertId, CancellationToken ct = default);
    Task CancelParticipationAsync(Guid alertId, CancellationToken ct = default);

    // Côté demandeur
    Task<IReadOnlyList<RequesterAlertSummary>> GetRequesterAlertsAsync(UserProfile requester, CancellationToken ct = default);
    Task<Alert> CreateAlertAsync(NewAlertRequest request, UserProfile requester, CancellationToken ct = default);
}

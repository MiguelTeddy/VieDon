using VieDon.Core.Compatibility;
using VieDon.Core.Models;

namespace VieDon.App.Services;

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Alertes d'EXEMPLE en mémoire (établissements et chiffres fictifs).
/// Applique le ciblage RG1 / RG2 et le quota RG4. À remplacer par un client de l'API.
/// </summary>
public sealed class InMemoryAlertService : IAlertService
{
    private sealed class State
    {
        public required Alert Alert { get; init; }
        public int Accepted { get; set; }
        public Participation? Mine { get; set; }
        public string? OwnerPhone { get; init; }
        public int RadiusKm { get; init; } = 10;
    }

    private readonly Dictionary<Guid, State> _alerts = new();

    public InMemoryAlertService()
    {
        var now = DateTimeOffset.Now;
        Add(UrgencyLevel.Critical, BloodGroup.O, Rhesus.Positive, 3, "Hôpital Central", true, 2.4, now.AddMinutes(-4), "Centre-ville", false, true);
        Add(UrgencyLevel.Urgent, BloodGroup.A, Rhesus.Positive, 1, "Clinique du Centre", true, 5.0, now.AddMinutes(-35), "Quartier nord", false, false);
        Add(UrgencyLevel.Normal, BloodGroup.B, Rhesus.Negative, 2, "Centre de santé Exemple", true, 7.8, now.AddHours(-3), "Quartier est", false, false);
        Add(UrgencyLevel.Urgent, BloodGroup.AB, Rhesus.Positive, 1, "Particulier (non vérifié)", false, 3.1, now.AddMinutes(-12), "Centre-ville", true, true);
    }

    private void Add(UrgencyLevel level, BloodGroup g, Rhesus r, int pouches, string place, bool verified,
        double km, DateTimeOffset created, string zone, bool individual, bool compatible)
    {
        var alert = new Alert(Guid.NewGuid(), level, g, r, pouches, new Establishment(place, "Ville", verified),
            km, created, AlertStatus.Active, zone, individual, compatible);
        _alerts[alert.Id] = new State { Alert = alert };
    }

    public async Task<IReadOnlyList<Alert>> GetNearbyAlertsAsync(UserProfile donor, CancellationToken ct = default)
    {
        await Task.Delay(350, ct);
        return _alerts.Values.Select(s => s.Alert)
            .Where(a => a.Status == AlertStatus.Active)
            .Where(a => CompatibilityTable.IsTargeted(donor.Group, donor.RhesusFactor, a.Group, a.RhesusFactor, a.IncludeCompatible))
            .OrderByDescending(a => a.Level)
            .ThenBy(a => a.DistanceKm)
            .ToList();
    }

    public async Task<Alert?> GetAlertAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(150, ct);
        return _alerts.TryGetValue(id, out var s) ? s.Alert : null;
    }

    public async Task<AcceptResult> AcceptAsync(Guid alertId, UserProfile donor, CancellationToken ct = default)
    {
        await Task.Delay(400, ct);
        if (!_alerts.TryGetValue(alertId, out var s) || s.Alert.Status != AlertStatus.Active)
            return new AcceptResult(AcceptStatus.NoLongerActive);

        if (s.Mine is not null)
            return new AcceptResult(AcceptStatus.Confirmed, s.Mine);

        // RG4 : quand le besoin est couvert, les nouvelles acceptations passent en liste d'attente.
        if (s.Accepted >= s.Alert.Pouches)
            return new AcceptResult(AcceptStatus.WaitingList);

        s.Accepted++;
        s.Mine = new Participation(
            alertId,
            $"VD-{Random.Shared.Next(1000, 9999)}",
            s.Alert.Establishment.Name,
            $"Adresse d'exemple, {s.Alert.Establishment.City}", // RG17 : toujours un établissement de santé
            DateTimeOffset.Now.AddHours(2));
        return new AcceptResult(AcceptStatus.Confirmed, s.Mine);
    }

    public async Task<Participation?> GetParticipationAsync(Guid alertId, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        return _alerts.TryGetValue(alertId, out var s) ? s.Mine : null;
    }

    public async Task CancelParticipationAsync(Guid alertId, CancellationToken ct = default)
    {
        await Task.Delay(200, ct);
        if (_alerts.TryGetValue(alertId, out var s) && s.Mine is not null)
        {
            s.Mine = null;
            s.Accepted = Math.Max(0, s.Accepted - 1); // annulation à l'avance : sans pénalité (RG14)
        }
    }

    public async Task<IReadOnlyList<RequesterAlertSummary>> GetRequesterAlertsAsync(UserProfile requester, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);
        return _alerts.Values
            .Where(s => s.OwnerPhone == requester.Phone)
            .OrderByDescending(s => s.Alert.CreatedAt)
            // « Contactés » est simulé ici ; l'API réelle le calculera depuis le ciblage.
            .Select(s => new RequesterAlertSummary(s.Alert, Math.Max(1, s.RadiusKm * 3), s.Accepted))
            .ToList();
    }

    public async Task<Alert> CreateAlertAsync(NewAlertRequest request, UserProfile requester, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var verified = requester.Kind == RequesterKind.Establishment && requester.Verification == VerificationStatus.Verified;
        var alert = new Alert(
            Guid.NewGuid(), request.Level, request.Group, request.RhesusFactor, request.Pouches,
            new Establishment(request.PlaceName, "Ville", verified),
            Math.Round(1 + Random.Shared.NextDouble() * Math.Max(0, request.RadiusKm - 1), 1),
            DateTimeOffset.Now, AlertStatus.Active, "Centre-ville",
            requester.Kind == RequesterKind.Individual, request.IncludeCompatible);
        _alerts[alert.Id] = new State { Alert = alert, OwnerPhone = requester.Phone, RadiusKm = request.RadiusKm };
        return alert;
    }
}

using VieDon.Core.Compatibility;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.Services;

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Alertes d'EXEMPLE en mémoire (établissements et chiffres fictifs).
/// Applique le ciblage RG1 / RG2, le quota RG4 et la validation des dons RG10. À remplacer par un client de l'API.
/// </summary>
public sealed class InMemoryAlertService : IAlertService
{
    private sealed record DonorInfo(string Name, string GroupLabel, string Place, string City);

    private sealed class State
    {
        public required Alert Alert { get; set; }
        public int Accepted { get; set; }
        public Dictionary<string, Participation> Mine { get; } = new();   // clé : téléphone du donneur
        public Dictionary<string, DonorInfo> Donors { get; } = new();
        public HashSet<string> Validated { get; } = new();
        public string? OwnerPhone { get; init; }
        public int RadiusKm { get; init; } = 10;
    }

    private readonly Dictionary<Guid, State> _alerts = new();
    private readonly IDonationService _donations;

    public InMemoryAlertService(IDonationService donations)
    {
        _donations = donations;
        var now = DateTimeOffset.Now;

        // Alertes en cours
        Add(DemoData.HopitalCentral, UrgencyLevel.Critical, BloodGroup.O, Rhesus.Positive, 3, "Hôpital Central", true, 2.4, now.AddMinutes(-4), "Centre-ville", false, true, accepted: 1);
        Add(DemoData.CliniqueCentre, UrgencyLevel.Urgent, BloodGroup.A, Rhesus.Positive, 1, "Clinique du Centre", true, 5.0, now.AddMinutes(-35), "Quartier nord", false, false);
        Add(null, UrgencyLevel.Normal, BloodGroup.B, Rhesus.Negative, 2, "Centre de santé Exemple", true, 7.8, now.AddHours(-3), "Quartier est", false, false);
        Add(DemoData.Paul, UrgencyLevel.Urgent, BloodGroup.AB, Rhesus.Positive, 1, "Particulier (non vérifié)", false, 3.1, now.AddMinutes(-12), "Centre-ville", true, true);

        // Historique : Hôpital Central
        Past(DemoData.HopitalCentral, UrgencyLevel.Urgent, BloodGroup.A, Rhesus.Positive, 2, "Hôpital Central", true, 3, AlertStatus.Filled, 2, false);
        Past(DemoData.HopitalCentral, UrgencyLevel.Critical, BloodGroup.O, Rhesus.Negative, 3, "Hôpital Central", true, 8, AlertStatus.Filled, 3, true);
        Past(DemoData.HopitalCentral, UrgencyLevel.Normal, BloodGroup.B, Rhesus.Positive, 1, "Hôpital Central", true, 15, AlertStatus.Filled, 1, false);
        Past(DemoData.HopitalCentral, UrgencyLevel.Urgent, BloodGroup.AB, Rhesus.Negative, 2, "Hôpital Central", true, 21, AlertStatus.Expired, 1, true);
        Past(DemoData.HopitalCentral, UrgencyLevel.Normal, BloodGroup.A, Rhesus.Negative, 1, "Hôpital Central", true, 30, AlertStatus.Cancelled, 0, false);
        Past(DemoData.HopitalCentral, UrgencyLevel.Critical, BloodGroup.O, Rhesus.Positive, 2, "Hôpital Central", true, 45, AlertStatus.Filled, 2, true);

        // Historique : Clinique du Centre
        Past(DemoData.CliniqueCentre, UrgencyLevel.Urgent, BloodGroup.B, Rhesus.Negative, 1, "Clinique du Centre", true, 5, AlertStatus.Filled, 1, false);
        Past(DemoData.CliniqueCentre, UrgencyLevel.Normal, BloodGroup.O, Rhesus.Positive, 2, "Clinique du Centre", true, 12, AlertStatus.Expired, 1, false);

        // Historique : Paul (individu)
        Past(DemoData.Paul, UrgencyLevel.Normal, BloodGroup.A, Rhesus.Positive, 1, "Hôpital Central", false, 20, AlertStatus.Filled, 1, false, individual: true);
        Past(DemoData.Paul, UrgencyLevel.Urgent, BloodGroup.B, Rhesus.Positive, 1, "Clinique du Centre", false, 40, AlertStatus.Expired, 0, false, individual: true);
    }

    private void Add(string? owner, UrgencyLevel level, BloodGroup g, Rhesus r, int pouches, string place, bool verified,
        double km, DateTimeOffset created, string zone, bool individual, bool compatible,
        AlertStatus status = AlertStatus.Active, int accepted = 0, int radius = 10)
    {
        var alert = new Alert(Guid.NewGuid(), level, g, r, pouches, new Establishment(place, "Ville", verified),
            km, created, status, zone, individual, compatible);
        _alerts[alert.Id] = new State { Alert = alert, OwnerPhone = owner, Accepted = accepted, RadiusKm = radius };
    }

    private void Past(string owner, UrgencyLevel level, BloodGroup g, Rhesus r, int pouches, string place, bool verified,
        int daysAgo, AlertStatus status, int accepted, bool compatible, bool individual = false) =>
        Add(owner, level, g, r, pouches, place, verified, 3.0, DateTimeOffset.Now.AddDays(-daysAgo), "Centre-ville",
            individual, compatible, status, accepted);

    private string NewCode()
    {
        string code;
        do { code = $"VD-{Random.Shared.Next(1000, 9999)}"; }
        while (_alerts.Values.Any(s => s.Mine.Values.Any(p => p.Code == code)));
        return code;
    }

    public async Task<IReadOnlyList<Alert>> GetNearbyAlertsAsync(UserProfile donor, CancellationToken ct = default)
    {
        await Task.Delay(350, ct);
        if (!donor.IsAvailable) return Array.Empty<Alert>(); // un donneur indisponible ne reçoit pas d'alertes
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

        if (s.Mine.TryGetValue(donor.Phone, out var existing))
            return new AcceptResult(AcceptStatus.Confirmed, existing);

        // RG4 : quand le besoin est couvert, les nouvelles acceptations passent en liste d'attente.
        if (s.Accepted >= s.Alert.Pouches)
            return new AcceptResult(AcceptStatus.WaitingList);

        s.Accepted++;
        var participation = new Participation(
            alertId,
            NewCode(),
            s.Alert.Establishment.Name,
            $"Adresse d'exemple, {s.Alert.Establishment.City}", // RG17 : toujours un établissement de santé
            DateTimeOffset.Now.AddHours(2));
        s.Mine[donor.Phone] = participation;
        s.Donors[donor.Phone] = new DonorInfo(donor.FirstName, donor.GroupLabel, s.Alert.Establishment.Name, s.Alert.Establishment.City);

        // Le don apparaît dans l'historique du donneur « en attente de validation » (RG10).
        await _donations.AddPendingAsync(donor.Phone, alertId, s.Alert.Establishment.Name, s.Alert.Establishment.City, ct);
        return new AcceptResult(AcceptStatus.Confirmed, participation);
    }

    public async Task<Participation?> GetParticipationAsync(Guid alertId, UserProfile donor, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        return _alerts.TryGetValue(alertId, out var s) && s.Mine.TryGetValue(donor.Phone, out var p) ? p : null;
    }

    public async Task CancelParticipationAsync(Guid alertId, UserProfile donor, CancellationToken ct = default)
    {
        await Task.Delay(200, ct);
        if (_alerts.TryGetValue(alertId, out var s) && s.Mine.ContainsKey(donor.Phone) && !s.Validated.Contains(donor.Phone))
        {
            s.Mine.Remove(donor.Phone);
            s.Donors.Remove(donor.Phone);
            s.Accepted = Math.Max(0, s.Accepted - 1); // annulation à l'avance : sans pénalité (RG14)
            await _donations.RemovePendingAsync(donor.Phone, alertId, ct);
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

    public async Task<DonationValidation> ValidateDonationAsync(string code, UserProfile validator, CancellationToken ct = default)
    {
        await Task.Delay(400, ct);
        if (!AlertPolicy.CanValidateDonation(validator))
            return new DonationValidation(ValidationStatus.NotAllowed);

        var wanted = (code ?? string.Empty).Trim();
        foreach (var s in _alerts.Values)
        {
            foreach (var entry in s.Mine)
            {
                if (!string.Equals(entry.Value.Code, wanted, StringComparison.OrdinalIgnoreCase)) continue;

                var phone = entry.Key;
                var info = s.Donors[phone];
                if (s.Validated.Contains(phone))
                    return new DonationValidation(ValidationStatus.AlreadyValidated, phone, info.Name, info.GroupLabel, info.Place);

                s.Validated.Add(phone);
                await _donations.ValidateAsync(phone, s.Alert.Id, ct);
                if (s.Validated.Count >= s.Alert.Pouches)
                    s.Alert = s.Alert with { Status = AlertStatus.Filled };
                return new DonationValidation(ValidationStatus.Validated, phone, info.Name, info.GroupLabel, info.Place);
            }
        }
        return new DonationValidation(ValidationStatus.UnknownCode);
    }
}

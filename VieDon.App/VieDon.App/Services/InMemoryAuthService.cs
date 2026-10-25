using VieDon.Core.Validation;
using VieDon.Core.Models;

namespace VieDon.App.Services;

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Simule le serveur en mémoire : rien n'est conservé après la fermeture
/// et le code SMS est fixe. À remplacer par un client de l'API (RG15 : vrai code par SMS).
/// </summary>
public sealed class InMemoryAuthService : IAuthService
{
    public const string DevSmsCode = "123456";

    private sealed record Account(string Password, UserProfile Profile);
    private readonly Dictionary<string, Account> _accounts = new();

    public InMemoryAuthService()
    {
        // Comptes d'exemple (voir DemoData) : mot de passe commun DemoData.Password.
        SeedDonor(DemoData.Aminata, "Aminata Nkoulou", BloodGroup.O, Rhesus.Positive, GroupStatus.Confirmed, new DateTime(1992, 4, 12));
        SeedDonor(DemoData.Marc, "Marc Tchoumi", BloodGroup.O, Rhesus.Negative, GroupStatus.Confirmed, new DateTime(1985, 9, 3));
        SeedDonor(DemoData.JeanPaul, "Jean-Paul Essomba", BloodGroup.A, Rhesus.Negative, GroupStatus.Confirmed, new DateTime(1990, 1, 25));
        SeedDonor(DemoData.Fatou, "Fatou Diallo", BloodGroup.AB, Rhesus.Positive, GroupStatus.Confirmed, new DateTime(1998, 6, 18));
        SeedDonor(DemoData.Nadia, "Nadia Bello", BloodGroup.O, Rhesus.Positive, GroupStatus.Declared, new DateTime(2000, 11, 2));

        SeedRequester(DemoData.HopitalCentral, RequesterKind.Establishment, "Dr Ngono", "Hôpital Central",
            EstablishmentType.Hospital, "Avenue de la Santé (adresse d'exemple)", VerificationStatus.Verified);
        SeedRequester(DemoData.CliniqueCentre, RequesterKind.Establishment, "Dr Fouda", "Clinique du Centre",
            EstablishmentType.Clinic, "Rue du Centre (adresse d'exemple)", VerificationStatus.Verified);
        SeedRequester(DemoData.Paul, RequesterKind.Individual, "Paul Mbarga", null, null, "Quartier nord", VerificationStatus.Verified);
        SeedRequester(DemoData.Association, RequesterKind.Establishment, "Sandrine Ewane", "Association Entraide Exemple",
            EstablishmentType.Association, "Place du Marché (adresse d'exemple)", VerificationStatus.Pending);
    }

    private void SeedDonor(string phone, string name, BloodGroup group, Rhesus rhesus, GroupStatus status, DateTime birth) =>
        _accounts[phone] = new Account(DemoData.Password,
            new UserProfile(name, phone, UserSpace.Donor, group, rhesus, status, "Ville", birth));

    private void SeedRequester(string phone, RequesterKind kind, string name, string? org, EstablishmentType? type,
        string address, VerificationStatus verification) =>
        _accounts[phone] = new Account(DemoData.Password,
            new UserProfile(name, phone, UserSpace.Requester, null, null, GroupStatus.Unknown, address, default,
                kind, verification, org, type));

    public async Task<LoginResult> LoginAsync(string identifier, string password, CancellationToken ct = default)
    {
        await Task.Delay(400, ct);
        var key = Validators.NormalizePhone(identifier);
        if (_accounts.TryGetValue(key, out var account) && account.Password == password)
            return new LoginResult(LoginStatus.Success, account.Profile);
        return new LoginResult(LoginStatus.InvalidCredentials);
    }

    public async Task<bool> SendSmsCodeAsync(string phone, CancellationToken ct = default)
    {
        await Task.Delay(300, ct);
        return Validators.IsValidPhone(phone);
    }

    public async Task<bool> VerifySmsCodeAsync(string phone, string code, CancellationToken ct = default)
    {
        await Task.Delay(300, ct);
        return code == DevSmsCode;
    }

    public async Task<RegistrationResult> RegisterDonorAsync(DonorRegistration r, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var key = Validators.NormalizePhone(r.Phone);
        if (_accounts.ContainsKey(key)) // RG15 : un numéro = un compte
            return new RegistrationResult(RegistrationStatus.PhoneAlreadyUsed);

        // RG8 / RG11 : le groupe saisi est « Déclaré » ; si le donneur ne le connaît pas, il reste dans le réseau sans groupe.
        var status = r.Group is null ? GroupStatus.Unknown : GroupStatus.Declared;
        var profile = new UserProfile(r.FullName.Trim(), key, UserSpace.Donor, r.Group, r.RhesusFactor, status, r.City.Trim(), r.BirthDate);
        _accounts[key] = new Account(r.Password, profile);
        return new RegistrationResult(RegistrationStatus.Success, profile);
    }

    public async Task<RegistrationResult> RegisterRequesterAsync(RequesterRegistration r, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var key = Validators.NormalizePhone(r.Phone);
        if (_accounts.ContainsKey(key))
            return new RegistrationResult(RegistrationStatus.PhoneAlreadyUsed);

        // RG15 : un établissement fournit un justificatif vérifié par une personne avant de lancer une alerte.
        // Un individu est vérifié par son téléphone ; ses alertes restent marquées « Non vérifiée ».
        var verification = r.Kind == RequesterKind.Establishment ? VerificationStatus.Pending : VerificationStatus.Verified;
        var profile = new UserProfile(r.FullName.Trim(), key, UserSpace.Requester, null, null, GroupStatus.Unknown,
            r.Address?.Trim() ?? string.Empty, default, r.Kind, verification, r.OrganizationName?.Trim(), r.OrganizationType);
        _accounts[key] = new Account(r.Password, profile);
        return new RegistrationResult(RegistrationStatus.Success, profile);
    }

    public Task<UserProfile?> GetProfileAsync(string phone, CancellationToken ct = default) =>
        Task.FromResult(_accounts.TryGetValue(Validators.NormalizePhone(phone), out var a) ? a.Profile : null);

    public Task ConfirmGroupAsync(string phone, CancellationToken ct = default)
    {
        var key = Validators.NormalizePhone(phone);
        if (_accounts.TryGetValue(key, out var a) && a.Profile.Group is not null)
            _accounts[key] = a with { Profile = a.Profile with { GroupStatus = GroupStatus.Confirmed } };
        return Task.CompletedTask;
    }

    public async Task SetAvailabilityAsync(string phone, bool available, CancellationToken ct = default)
    {
        await Task.Delay(200, ct);
        var key = Validators.NormalizePhone(phone);
        if (_accounts.TryGetValue(key, out var a))
            _accounts[key] = a with { Profile = a.Profile with { IsAvailable = available } };
    }

    public async Task DeleteAccountAsync(string phone, CancellationToken ct = default)
    {
        await Task.Delay(300, ct);
        _accounts.Remove(Validators.NormalizePhone(phone));
    }

    public Task<IReadOnlyList<UserProfile>> GetAllProfilesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<UserProfile>>(_accounts.Values.Select(a => a.Profile).OrderBy(p => p.Phone).ToList());

    public Task SimulateVerificationAsync(string phone, bool approve, CancellationToken ct = default)
    {
        var key = Validators.NormalizePhone(phone);
        if (_accounts.TryGetValue(key, out var a))
        {
            var status = approve ? VerificationStatus.Verified : VerificationStatus.Rejected;
            _accounts[key] = a with { Profile = a.Profile with { Verification = status } };
        }
        return Task.CompletedTask;
    }
}

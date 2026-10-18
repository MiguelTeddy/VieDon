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
            r.Address?.Trim() ?? string.Empty, default, r.Kind, verification, r.OrganizationName?.Trim());
        _accounts[key] = new Account(r.Password, profile);
        return new RegistrationResult(RegistrationStatus.Success, profile);
    }

    public Task<UserProfile?> GetProfileAsync(string phone, CancellationToken ct = default) =>
        Task.FromResult(_accounts.TryGetValue(Validators.NormalizePhone(phone), out var a) ? a.Profile : null);

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

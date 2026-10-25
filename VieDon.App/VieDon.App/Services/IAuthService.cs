using VieDon.Core.Models;

namespace VieDon.App.Services;

public enum LoginStatus { Success, InvalidCredentials, AccountSuspended, NetworkError }
public enum RegistrationStatus { Success, PhoneAlreadyUsed, NetworkError }

public sealed record LoginResult(LoginStatus Status, UserProfile? Profile = null);
public sealed record RegistrationResult(RegistrationStatus Status, UserProfile? Profile = null);

public sealed record DonorRegistration(
    string FullName,
    string Phone,
    string Password,
    BloodGroup? Group,
    Rhesus? RhesusFactor,
    string City,
    DateTime BirthDate);

public sealed record RequesterRegistration(
    RequesterKind Kind,
    string FullName,
    string Phone,
    string Password,
    EstablishmentType? OrganizationType,
    string? OrganizationName,
    string? Address,
    string? ProfessionalEmail,
    string? JustificatifFileName);

/// <summary>Contrat d'authentification. L'implémentation réelle appellera l'API VieDon.</summary>
public interface IAuthService
{
    Task<LoginResult> LoginAsync(string identifier, string password, CancellationToken ct = default);
    Task<bool> SendSmsCodeAsync(string phone, CancellationToken ct = default);
    Task<bool> VerifySmsCodeAsync(string phone, string code, CancellationToken ct = default);
    Task<RegistrationResult> RegisterDonorAsync(DonorRegistration registration, CancellationToken ct = default);
    Task<RegistrationResult> RegisterRequesterAsync(RequesterRegistration registration, CancellationToken ct = default);

    /// <summary>Relit le profil (par exemple après la vérification d'un établissement).</summary>
    Task<UserProfile?> GetProfileAsync(string phone, CancellationToken ct = default);

    /// <summary>RG8 : le groupe devient « Confirmé » quand un centre valide un don.</summary>
    Task ConfirmGroupAsync(string phone, CancellationToken ct = default);

    Task SetAvailabilityAsync(string phone, bool available, CancellationToken ct = default);

    /// <summary>RG23 : suppression du compte et des données.</summary>
    Task DeleteAccountAsync(string phone, CancellationToken ct = default);

    /// <summary>DÉVELOPPEMENT : liste tous les comptes (écran d'administration de démonstration).</summary>
    Task<IReadOnlyList<UserProfile>> GetAllProfilesAsync(CancellationToken ct = default);

    /// <summary>DÉVELOPPEMENT : simule la décision d'un vérificateur humain (RG15).</summary>
    Task SimulateVerificationAsync(string phone, bool approve, CancellationToken ct = default);
}

using VieDon.Core.Models;

namespace VieDon.App.Services;

/// <summary>Historique de dons, niveaux et avantages. L'implémentation réelle appellera l'API VieDon.</summary>
public interface IDonationService
{
    Task<IReadOnlyList<Donation>> GetDonationsAsync(string phone, CancellationToken ct = default);

    // Cycle de vie d'un don lié à une alerte : accepté (en attente) puis validé par un centre (RG10).
    Task AddPendingAsync(string phone, Guid alertId, string place, string city, CancellationToken ct = default);
    Task RemovePendingAsync(string phone, Guid alertId, CancellationToken ct = default);
    Task<Donation?> ValidateAsync(string phone, Guid alertId, CancellationToken ct = default);

    // Avantages (RG22 : non monétaires)
    Task<IReadOnlyList<RewardItem>> GetRewardsAsync(string phone, CancellationToken ct = default);
    Task<RewardItem?> GetRewardAsync(string phone, Guid rewardId, CancellationToken ct = default);
    Task<RewardItem?> UseRewardAsync(string phone, Guid rewardId, CancellationToken ct = default);
}

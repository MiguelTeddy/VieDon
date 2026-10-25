namespace VieDon.Core.Models;

public enum RewardStatus { Available, Locked, Expired, Used }

/// <summary>Avantage partenaire NON monétaire (RG22), débloqué par un niveau obtenu avec des dons validés.</summary>
public sealed record Reward(
    Guid Id,
    string Partner,
    string Title,
    string Description,
    string Conditions,
    DonorLevel RequiredLevel,
    DateTime ValidUntil);

public sealed record RewardItem(Reward Reward, RewardStatus Status, string? UseCode = null);

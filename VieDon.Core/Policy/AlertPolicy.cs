using VieDon.Core.Models;

namespace VieDon.Core.Policy;

/// <summary>Qui peut lancer quelle alerte (RG3, RG15).</summary>
public static class AlertPolicy
{
    /// <summary>Un compte demandeur doit être vérifié avant de lancer une alerte.</summary>
    public static bool CanLaunch(UserProfile profile) =>
        profile.Space == UserSpace.Requester && profile.Verification == VerificationStatus.Verified;

    /// <summary>RG3 : le niveau Critique est réservé aux établissements vérifiés.</summary>
    public static IReadOnlyList<UrgencyLevel> AllowedLevels(UserProfile profile)
    {
        if (!CanLaunch(profile)) return Array.Empty<UrgencyLevel>();
        return profile.Kind == RequesterKind.Establishment
            ? new[] { UrgencyLevel.Normal, UrgencyLevel.Urgent, UrgencyLevel.Critical }
            : new[] { UrgencyLevel.Normal, UrgencyLevel.Urgent };
    }

    public static bool IsLevelAllowed(UserProfile profile, UrgencyLevel level) =>
        AllowedLevels(profile).Contains(level);
}

using VieDon.Core.Models;

namespace VieDon.Core.Policy;

public sealed record BadgeInfo(string Label, bool Earned, string Hint);

/// <summary>Niveaux de reconnaissance : Bronze (1 don validé), Argent (3), Or (6), Platine (10). Seuils à valider (cahier, section 11).</summary>
public static class LevelPolicy
{
    public static DonorLevel FromDonations(int validatedDonations)
    {
        if (validatedDonations >= 10) return DonorLevel.Platinum;
        if (validatedDonations >= 6) return DonorLevel.Gold;
        if (validatedDonations >= 3) return DonorLevel.Silver;
        if (validatedDonations >= 1) return DonorLevel.Bronze;
        return DonorLevel.None;
    }

    public static int Threshold(DonorLevel level)
    {
        switch (level)
        {
            case DonorLevel.Bronze: return 1;
            case DonorLevel.Silver: return 3;
            case DonorLevel.Gold: return 6;
            case DonorLevel.Platinum: return 10;
            default: return 0;
        }
    }

    public static DonorLevel? Next(DonorLevel level)
    {
        switch (level)
        {
            case DonorLevel.None: return DonorLevel.Bronze;
            case DonorLevel.Bronze: return DonorLevel.Silver;
            case DonorLevel.Silver: return DonorLevel.Gold;
            case DonorLevel.Gold: return DonorLevel.Platinum;
            default: return null;
        }
    }

    public static string Label(DonorLevel level)
    {
        switch (level)
        {
            case DonorLevel.Bronze: return "Bronze";
            case DonorLevel.Silver: return "Argent";
            case DonorLevel.Gold: return "Or";
            case DonorLevel.Platinum: return "Platine";
            default: return "Aucun niveau";
        }
    }

    /// <summary>Progression (0 à 1) vers le prochain niveau ; 1 au niveau maximum.</summary>
    public static double Progress(int validatedDonations)
    {
        var current = FromDonations(validatedDonations);
        var next = Next(current);
        if (next is null) return 1.0;
        var from = Threshold(current);
        var to = Threshold(next.Value);
        return Math.Clamp((double)(validatedDonations - from) / (to - from), 0.0, 1.0);
    }

    /// <summary>Nombre de dons validés restant avant le prochain niveau (0 au niveau maximum).</summary>
    public static int Remaining(int validatedDonations)
    {
        var next = Next(FromDonations(validatedDonations));
        return next is null ? 0 : Math.Max(0, Threshold(next.Value) - validatedDonations);
    }

    public static IReadOnlyList<BadgeInfo> Badges(int validatedDonations, bool groupConfirmed) => new[]
    {
        new BadgeInfo("Premier don", validatedDonations >= 1, "Réalise ton premier don validé"),
        new BadgeInfo("3 dons", validatedDonations >= 3, "Atteins 3 dons validés"),
        new BadgeInfo("6 dons", validatedDonations >= 6, "Atteins 6 dons validés"),
        new BadgeInfo("10 dons", validatedDonations >= 10, "Atteins 10 dons validés"),
        new BadgeInfo("Groupe confirmé", groupConfirmed, "Fais valider ton groupe par un centre")
    };
}

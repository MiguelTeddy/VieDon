using VieDon.Core.Models;

namespace VieDon.Core.Policy;

public sealed record DonationStats(
    int ValidatedCount,
    int PendingCount,
    int PresencesCount,
    DateTime? LastDonation,
    DateTime? NextPossible,
    bool CanDonateNow,
    DonorLevel Level);

public static class DonationStatsCalculator
{
    /// <summary>RG10 : seuls les dons validés comptent pour le total, le niveau et le délai avant le prochain don.</summary>
    public static DonationStats Compute(IEnumerable<Donation> donations, DateTime today)
    {
        var list = donations.ToList();
        var validated = list.Where(d => d.Status == DonationStatus.Validated).ToList();
        var pending = list.Count(d => d.Status == DonationStatus.Pending);
        var attended = list.Count(d => d.Status == DonationStatus.AttendedNoDonation);

        DateTime? last = validated.Count == 0 ? null : validated.Max(d => d.Date);
        DateTime? next = last?.AddDays(AppConstants.DonationIntervalDays);
        var canDonate = next is null || next.Value.Date <= today.Date;

        return new DonationStats(validated.Count, pending, validated.Count + attended, last, next, canDonate,
            LevelPolicy.FromDonations(validated.Count));
    }
}

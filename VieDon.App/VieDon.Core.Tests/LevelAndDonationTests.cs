using VieDon.Core.Models;
using VieDon.Core.Policy;
using Xunit;

namespace VieDon.Core.Tests;

public class LevelAndDonationTests
{
    [Theory]
    [InlineData(0, DonorLevel.None)]
    [InlineData(1, DonorLevel.Bronze)]
    [InlineData(2, DonorLevel.Bronze)]
    [InlineData(3, DonorLevel.Silver)]
    [InlineData(5, DonorLevel.Silver)]
    [InlineData(6, DonorLevel.Gold)]
    [InlineData(9, DonorLevel.Gold)]
    [InlineData(10, DonorLevel.Platinum)]
    [InlineData(25, DonorLevel.Platinum)]
    public void Level_follows_validated_donations(int donations, DonorLevel expected) =>
        Assert.Equal(expected, LevelPolicy.FromDonations(donations));

    [Fact]
    public void Progress_and_remaining_towards_next_level()
    {
        Assert.Equal(0.0, LevelPolicy.Progress(0));
        Assert.Equal(1, LevelPolicy.Remaining(0));
        Assert.Equal(3, LevelPolicy.Remaining(7)); // niveau Or : encore 3 dons pour atteindre Platine (10)
        Assert.Equal(0.25, LevelPolicy.Progress(7), 3);
        Assert.Equal(1.0, LevelPolicy.Progress(10));
        Assert.Equal(0, LevelPolicy.Remaining(12));
    }

    [Fact]
    public void Next_level_of_platinum_is_none() => Assert.Null(LevelPolicy.Next(DonorLevel.Platinum));

    [Fact]
    public void Badges_reflect_donations_and_group_status()
    {
        var badges = LevelPolicy.Badges(3, groupConfirmed: true);
        Assert.Contains(badges, b => b.Label == "3 dons" && b.Earned);
        Assert.Contains(badges, b => b.Label == "6 dons" && !b.Earned);
        Assert.Contains(badges, b => b.Label == "Groupe confirmé" && b.Earned);
    }

    private static Donation D(DateTime date, DonationStatus status) =>
        new(Guid.NewGuid(), date, "Centre Exemple", "Ville", status);

    [Fact]
    public void Only_validated_donations_count_RG10()
    {
        var today = new DateTime(2026, 10, 18);
        var stats = DonationStatsCalculator.Compute(new[]
        {
            D(today.AddDays(-100), DonationStatus.Validated),
            D(today.AddDays(-2), DonationStatus.Pending),
            D(today.AddDays(-50), DonationStatus.AttendedNoDonation)
        }, today);

        Assert.Equal(1, stats.ValidatedCount);
        Assert.Equal(1, stats.PendingCount);
        Assert.Equal(2, stats.PresencesCount);
        Assert.Equal(DonorLevel.Bronze, stats.Level);
    }

    [Fact]
    public void Next_donation_is_after_the_interval()
    {
        var today = new DateTime(2026, 10, 18);
        var recent = DonationStatsCalculator.Compute(new[] { D(today.AddDays(-10), DonationStatus.Validated) }, today);
        Assert.False(recent.CanDonateNow);
        Assert.Equal(today.AddDays(-10).AddDays(AppConstants.DonationIntervalDays), recent.NextPossible);

        var old = DonationStatsCalculator.Compute(new[] { D(today.AddDays(-AppConstants.DonationIntervalDays), DonationStatus.Validated) }, today);
        Assert.True(old.CanDonateNow);

        var none = DonationStatsCalculator.Compute(Array.Empty<Donation>(), today);
        Assert.True(none.CanDonateNow);
        Assert.Null(none.NextPossible);
    }

    [Fact]
    public void Only_a_verified_establishment_can_validate_donations()
    {
        UserProfile P(RequesterKind k, VerificationStatus v) =>
            new("Test", "600000000", UserSpace.Requester, null, null, GroupStatus.Unknown, "", default, k, v, null);
        Assert.True(AlertPolicy.CanValidateDonation(P(RequesterKind.Establishment, VerificationStatus.Verified)));
        Assert.False(AlertPolicy.CanValidateDonation(P(RequesterKind.Establishment, VerificationStatus.Pending)));
        Assert.False(AlertPolicy.CanValidateDonation(P(RequesterKind.Individual, VerificationStatus.Verified)));
    }
}

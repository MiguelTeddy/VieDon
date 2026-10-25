using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;
using Xunit;

namespace VieDon.Core.Tests;

public class AlertPolicyTests
{
    private static UserProfile Requester(RequesterKind kind, VerificationStatus status) =>
        new("Test", "600000000", UserSpace.Requester, null, null, GroupStatus.Unknown, "", default, kind, status, null);

    [Fact]
    public void Verified_establishment_can_launch_all_levels()
    {
        var p = Requester(RequesterKind.Establishment, VerificationStatus.Verified);
        Assert.True(AlertPolicy.CanLaunch(p));
        Assert.True(AlertPolicy.IsLevelAllowed(p, UrgencyLevel.Critical));
        Assert.Equal(3, AlertPolicy.AllowedLevels(p).Count);
    }

    [Fact]
    public void Individual_cannot_launch_critical_alerts()
    {
        var p = Requester(RequesterKind.Individual, VerificationStatus.Verified);
        Assert.True(AlertPolicy.IsLevelAllowed(p, UrgencyLevel.Urgent));
        Assert.False(AlertPolicy.IsLevelAllowed(p, UrgencyLevel.Critical));
    }

    [Theory]
    [InlineData(VerificationStatus.Pending)]
    [InlineData(VerificationStatus.Rejected)]
    public void Unverified_establishment_cannot_launch_any_alert(VerificationStatus status)
    {
        var p = Requester(RequesterKind.Establishment, status);
        Assert.False(AlertPolicy.CanLaunch(p));
        Assert.Empty(AlertPolicy.AllowedLevels(p));
    }

    [Fact]
    public void Donor_cannot_launch_alerts()
    {
        var donor = new UserProfile("Awa", "600000001", UserSpace.Donor, BloodGroup.O, Rhesus.Positive,
            GroupStatus.Declared, "Ville", new DateTime(1995, 1, 1));
        Assert.False(AlertPolicy.CanLaunch(donor));
    }

    [Fact]
    public void Establishment_type_labels_exist_for_every_value()
    {
        foreach (var t in Enum.GetValues<EstablishmentType>())
            Assert.False(string.IsNullOrWhiteSpace(Labels.EstablishmentTypeLabel(t)));
    }
}

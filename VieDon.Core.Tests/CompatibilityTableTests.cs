using VieDon.Core.Compatibility;
using VieDon.Core.Models;
using Xunit;

namespace VieDon.Core.Tests;

public class CompatibilityTableTests
{
    [Fact]
    public void ONegative_donor_is_compatible_with_every_patient()
    {
        foreach (var g in Enum.GetValues<BloodGroup>())
            foreach (var r in new[] { Rhesus.Positive, Rhesus.Negative })
                Assert.True(CompatibilityTable.IsCompatibleRedCells(BloodGroup.O, Rhesus.Negative, g, r));
    }

    [Fact]
    public void ABPositive_patient_accepts_all_eight_donor_types()
    {
        foreach (var g in Enum.GetValues<BloodGroup>())
            foreach (var r in new[] { Rhesus.Positive, Rhesus.Negative })
                Assert.True(CompatibilityTable.IsCompatibleRedCells(g, r, BloodGroup.AB, Rhesus.Positive));
    }

    [Fact]
    public void Compatibility_is_not_symmetric_and_rhesus_matters()
    {
        Assert.False(CompatibilityTable.IsCompatibleRedCells(BloodGroup.O, Rhesus.Positive, BloodGroup.O, Rhesus.Negative));
        Assert.True(CompatibilityTable.IsCompatibleRedCells(BloodGroup.O, Rhesus.Negative, BloodGroup.O, Rhesus.Positive));
        Assert.False(CompatibilityTable.IsCompatibleRedCells(BloodGroup.A, Rhesus.Positive, BloodGroup.AB, Rhesus.Negative));
        Assert.False(CompatibilityTable.IsCompatibleRedCells(BloodGroup.AB, Rhesus.Positive, BloodGroup.A, Rhesus.Positive));
    }

    [Fact]
    public void By_default_only_the_same_group_and_rhesus_is_targeted()
    {
        Assert.True(CompatibilityTable.IsTargeted(BloodGroup.A, Rhesus.Positive, BloodGroup.A, Rhesus.Positive, includeCompatible: false));
        Assert.False(CompatibilityTable.IsTargeted(BloodGroup.O, Rhesus.Negative, BloodGroup.A, Rhesus.Positive, includeCompatible: false));
        Assert.False(CompatibilityTable.IsTargeted(BloodGroup.A, Rhesus.Negative, BloodGroup.A, Rhesus.Positive, includeCompatible: false));
    }

    [Fact]
    public void Compatible_option_adds_compatible_groups()
    {
        Assert.True(CompatibilityTable.IsTargeted(BloodGroup.O, Rhesus.Negative, BloodGroup.A, Rhesus.Positive, includeCompatible: true));
    }

    [Fact]
    public void Donor_with_unknown_group_or_rhesus_is_never_targeted()
    {
        Assert.False(CompatibilityTable.IsTargeted(null, null, BloodGroup.O, Rhesus.Positive, true));
        Assert.False(CompatibilityTable.IsTargeted(BloodGroup.O, Rhesus.Unknown, BloodGroup.O, Rhesus.Positive, true));
    }

    [Fact]
    public void Targeted_groups_preview_for_ONegative_patient_without_option_is_only_ONegative()
    {
        var groups = CompatibilityTable.TargetedGroups(BloodGroup.O, Rhesus.Negative, includeCompatible: false);
        Assert.Single(groups);
        Assert.Equal((BloodGroup.O, Rhesus.Negative), groups[0]);
    }

    [Fact]
    public void Targeted_groups_preview_for_APositive_with_option_has_four_groups()
    {
        var groups = CompatibilityTable.TargetedGroups(BloodGroup.A, Rhesus.Positive, includeCompatible: true);
        Assert.Equal(4, groups.Count); // A+, A-, O+, O-
    }
}

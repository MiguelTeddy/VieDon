using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Validation;
using Xunit;

namespace VieDon.Core.Tests;

public class ValidatorsTests
{
    [Theory]
    [InlineData("612345678", true)]
    [InlineData("+237 6 12 34 56 78", true)]
    [InlineData("6 12 34 56 78", true)]
    [InlineData("1234567", false)]
    [InlineData("abcdefghij", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Phone_validation(string? value, bool expected) =>
        Assert.Equal(expected, Validators.IsValidPhone(value));

    [Theory]
    [InlineData("awa@example.org", true)]
    [InlineData("awa@", false)]
    [InlineData("pas un email", false)]
    public void Email_validation(string value, bool expected) =>
        Assert.Equal(expected, Validators.IsValidEmail(value));

    [Fact]
    public void Age_is_computed_on_the_birthday()
    {
        var birth = new DateTime(2000, 6, 15);
        Assert.Equal(25, Validators.AgeOn(birth, new DateTime(2026, 6, 14)));
        Assert.Equal(26, Validators.AgeOn(birth, new DateTime(2026, 6, 15)));
    }
}

public class LabelsTests
{
    [Fact]
    public void Group_label_includes_rhesus() =>
        Assert.Equal("O+", Labels.GroupLabel(BloodGroup.O, Rhesus.Positive));

    [Fact]
    public void Urgency_is_always_a_word() =>
        Assert.Equal("Critique", Labels.UrgencyLabel(UrgencyLevel.Critical));

    [Fact]
    public void Pouches_singular_and_plural()
    {
        Assert.Equal("1 poche", Labels.PouchesText(1));
        Assert.Equal("3 poches", Labels.PouchesText(3));
    }

    [Fact]
    public void Relative_time()
    {
        var now = new DateTimeOffset(2026, 10, 13, 12, 0, 0, TimeSpan.Zero);
        Assert.Equal("à l'instant", Labels.Ago(now.AddSeconds(-20), now));
        Assert.Equal("il y a 4 min", Labels.Ago(now.AddMinutes(-4), now));
        Assert.Equal("il y a 2 h", Labels.Ago(now.AddHours(-2), now));
    }
}

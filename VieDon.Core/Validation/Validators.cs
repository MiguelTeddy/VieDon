using System.Net.Mail;

namespace VieDon.Core.Validation;

public static class Validators
{
    public static string NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return new string(value.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }

    public static bool IsValidPhone(string? value)
    {
        var p = NormalizePhone(value);
        if (p.StartsWith('+')) p = p[1..];
        return p.Length is >= 8 and <= 15 && p.All(char.IsDigit);
    }

    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try { return new MailAddress(value.Trim()).Address == value.Trim(); }
        catch (FormatException) { return false; }
    }

    public static bool IsPhoneOrEmail(string? value) => IsValidPhone(value) || IsValidEmail(value);

    public static int AgeOn(DateTime birthDate, DateTime today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}

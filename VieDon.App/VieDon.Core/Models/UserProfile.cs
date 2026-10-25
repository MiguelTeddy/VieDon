namespace VieDon.Core.Models;

public sealed record UserProfile(
    string FullName,
    string Phone,
    UserSpace Space,
    BloodGroup? Group,
    Rhesus? RhesusFactor,
    GroupStatus GroupStatus,
    string City,
    DateTime BirthDate,
    RequesterKind? Kind = null,
    VerificationStatus Verification = VerificationStatus.Verified,
    string? OrganizationName = null,
    EstablishmentType? OrganizationType = null,
    bool IsAvailable = true)
{
    public bool IsRequester => Space == UserSpace.Requester;
    public string DisplayName => string.IsNullOrWhiteSpace(OrganizationName) ? FullName : OrganizationName!;

    public string FirstName
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : FullName;
        }
    }

    public string GroupLabel
    {
        get
        {
            if (Group is null) return "inconnu";
            var sign = RhesusFactor switch
            {
                Rhesus.Positive => "+",
                Rhesus.Negative => "-",
                _ => string.Empty
            };
            return $"{Group}{sign}";
        }
    }

    public string GroupStatusLabel => GroupStatus switch
    {
        GroupStatus.Confirmed => "Confirmé",
        GroupStatus.Pending => "En attente",
        GroupStatus.Declared => "Déclaré",
        _ => "À déterminer"
    };
}

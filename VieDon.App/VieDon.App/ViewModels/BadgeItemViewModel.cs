using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Badge obtenu ou à débloquer. Jamais la couleur seule : ✓ + mot, ou « verrouillé ».</summary>
public sealed class BadgeItemViewModel
{
    private readonly BadgeInfo _badge;
    public BadgeItemViewModel(BadgeInfo badge) => _badge = badge;

    public bool IsEarned => _badge.Earned;
    public bool IsLocked => !_badge.Earned;
    public string EarnedText => $"✓ {_badge.Label}";
    public string LockedText => $"{_badge.Label} (verrouillé)";
    public string Description => _badge.Earned ? $"Badge obtenu : {_badge.Label}" : $"Badge verrouillé : {_badge.Label}. {_badge.Hint}";
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VieDon.App.ViewModels;

/// <summary>Pastille de choix (groupe sanguin, rhésus). L'état sélectionné est dit en texte, pas par la couleur seule.</summary>
public partial class ChipOption : ObservableObject
{
    private readonly Action<ChipOption> _onSelect;

    public ChipOption(string label, string value, Action<ChipOption> onSelect)
    {
        Label = label;
        Value = value;
        _onSelect = onSelect;
    }

    public string Label { get; }
    public string Value { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    [NotifyPropertyChangedFor(nameof(Description))]
    private bool isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    [NotifyPropertyChangedFor(nameof(Description))]
    private bool isEnabled = true;

    /// <summary>Texte ajouté quand l'option est verrouillée (jamais la couleur seule).</summary>
    public string LockedHint { get; init; } = "réservé";

    public string DisplayText => !IsEnabled ? $"{Label} ({LockedHint})" : IsSelected ? $"✓ {Label}" : Label;
    public string Description => !IsEnabled ? $"{Label}, verrouillé, {LockedHint}" : IsSelected ? $"{Label}, sélectionné" : Label;

    [RelayCommand]
    private void Select() => _onSelect(this);
}

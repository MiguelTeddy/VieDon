using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Une marche du « parcours des niveaux » (Bronze, Argent, Or, Platine) avec sa médaille.</summary>
public sealed class MedalStepViewModel
{
    private readonly DonorLevel _level;

    public MedalStepViewModel(DonorLevel level, int validatedDonations, DonorLevel current)
    {
        _level = level;
        IsReached = validatedDonations >= LevelPolicy.Threshold(level);
        IsCurrent = level == current;
    }

    public bool IsReached { get; }
    public bool IsCurrent { get; }

    // Médaille en couleur si atteinte, médaille grise sinon.
    public string Image => IsReached ? UiAssets.Medal(_level) : "medal_none.png";
    public string Name => LevelPolicy.Label(_level);
    public string ThresholdText => Labels.DonationsText(LevelPolicy.Threshold(_level));

    // L'état se dit en mots, jamais par la couleur seule.
    public string StateText => IsCurrent ? "✓ Niveau actuel" : IsReached ? "✓ Atteint" : "Verrouillé";

    public string Description => $"{Name}, {ThresholdText}, {StateText}";
}

using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Carte d'un avantage partenaire (écran 16). Le statut se dit en mot et en symbole.</summary>
public sealed class RewardItemViewModel
{
    public RewardItemViewModel(RewardItem item, Action<RewardItemViewModel> open)
    {
        Item = item;
        OpenCommand = new RelayCommand(() => open(this));
    }

    public RewardItem Item { get; }
    public IRelayCommand OpenCommand { get; }
    public Reward Reward => Item.Reward;

    public Guid Id => Reward.Id;
    public string Title => Reward.Title;
    public string MedalImage => UiAssets.Medal(Reward.RequiredLevel);
    public string Partner => Reward.Partner;
    public string RequiredText => $"Niveau requis : {LevelPolicy.Label(Reward.RequiredLevel)}";
    public string ValidityText => $"Valable jusqu'au {Labels.DateText(Reward.ValidUntil)}";

    public bool IsAvailable => Item.Status == RewardStatus.Available;
    public bool IsUsed => Item.Status == RewardStatus.Used;
    public bool IsLocked => Item.Status == RewardStatus.Locked;
    public bool IsExpired => Item.Status == RewardStatus.Expired;

    public string StatusText => Item.Status switch
    {
        RewardStatus.Available => "✓ Disponible",
        RewardStatus.Used => $"✓ Déjà utilisé (code {Item.UseCode})",
        RewardStatus.Locked => $"▲ Verrouillé : niveau {LevelPolicy.Label(Reward.RequiredLevel)} requis",
        _ => "✕ Expiré"
    };

    public string ButtonDescription => $"Voir le détail de l'avantage {Title}, {Partner}. {StatusText}.";
}

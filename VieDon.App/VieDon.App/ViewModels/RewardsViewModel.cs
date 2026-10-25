using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 16 : Avantages et niveaux (niveau, progression, badges, carte de donneur, avantages partenaires).</summary>
public partial class RewardsViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IDonationService _donations;
    private readonly INavigationService _nav;

    public RewardsViewModel(ISessionService session, IDonationService donations, INavigationService nav)
    {
        _session = session;
        _donations = donations;
        _nav = nav;
    }

    public ObservableCollection<BadgeItemViewModel> Badges { get; } = new();
    public ObservableCollection<MedalStepViewModel> LevelSteps { get; } = new();

    [ObservableProperty] private string cardMedal = "medal_none.png";
    public ObservableCollection<RewardItemViewModel> Rewards { get; } = new();

    // Carte de donneur numérique
    [ObservableProperty] private string cardName = string.Empty;
    [ObservableProperty] private string cardGroup = string.Empty;
    [ObservableProperty] private string cardNumber = string.Empty;
    [ObservableProperty] private string cardLevel = string.Empty;

    // Niveau et progression
    [ObservableProperty] private string levelLine = string.Empty;
    [ObservableProperty] private string progressText = string.Empty;
    [ObservableProperty] private double progress;

    // État « aucun avantage débloqué » : le prochain palier est affiché
    [ObservableProperty] private bool showNoneUnlocked;
    [ObservableProperty] private string noneUnlockedText = string.Empty;

    [ObservableProperty] private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    public string RulesText => "Les avantages sont non monétaires et débloqués par des dons validés par un centre. Le don reste volontaire et gratuit.";

    [RelayCommand]
    private async Task LoadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var donations = await _donations.GetDonationsAsync(user.Phone);
            var stats = DonationStatsCalculator.Compute(donations, DateTime.Today);
            var count = stats.ValidatedCount;

            // Carte de donneur
            var digits = new string(user.Phone.Where(char.IsDigit).ToArray());
            CardName = user.FullName;
            CardGroup = $"Groupe {user.GroupLabel} - {user.GroupStatusLabel}";
            CardNumber = $"Carte n° DON-{(digits.Length >= 4 ? digits[^4..] : digits)}";
            CardLevel = stats.Level == DonorLevel.None
                ? "Aucun niveau pour l'instant"
                : $"★ Niveau {LevelPolicy.Label(stats.Level)}";

            CardMedal = UiAssets.Medal(stats.Level);
            LevelSteps.Clear();
            foreach (var step in new[] { DonorLevel.Bronze, DonorLevel.Silver, DonorLevel.Gold, DonorLevel.Platinum })
                LevelSteps.Add(new MedalStepViewModel(step, count, stats.Level));

            // Niveau et progression
            LevelLine = stats.Level == DonorLevel.None
                ? "Aucun don validé pour l'instant"
                : $"Niveau {LevelPolicy.Label(stats.Level)} - {Labels.DonationsText(count)}";
            Progress = LevelPolicy.Progress(count);
            var next = LevelPolicy.Next(stats.Level);
            ProgressText = next is null
                ? "Niveau maximum atteint. Merci pour ta générosité."
                : $"Encore {Labels.DonationsText(LevelPolicy.Remaining(count))} pour le niveau {LevelPolicy.Label(next.Value)}.";

            // Badges
            Badges.Clear();
            foreach (var b in LevelPolicy.Badges(count, user.GroupStatus == GroupStatus.Confirmed))
                Badges.Add(new BadgeItemViewModel(b));

            // Avantages
            var rewards = await _donations.GetRewardsAsync(user.Phone);
            Rewards.Clear();
            foreach (var r in rewards) Rewards.Add(new RewardItemViewModel(r, Open));

            ShowNoneUnlocked = stats.Level == DonorLevel.None;
            NoneUnlockedText = next is null
                ? string.Empty
                : $"Aucun avantage débloqué pour l'instant. Ton prochain palier : {LevelPolicy.Label(next.Value)} ({LevelPolicy.Threshold(next.Value)} don validé).";
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger tes avantages. Vérifie ta connexion.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Open(RewardItemViewModel item) =>
        _ = _nav.GoToAsync($"{Routes.RewardDetail}?id={item.Id}");
}

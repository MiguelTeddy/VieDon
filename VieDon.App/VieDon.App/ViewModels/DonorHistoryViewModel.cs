using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 11 : Historique de dons (dons validés, présences, prochain don possible).</summary>
public partial class DonorHistoryViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IDonationService _donations;
    private readonly INavigationService _nav;

    public DonorHistoryViewModel(ISessionService session, IDonationService donations, INavigationService nav)
    {
        _session = session;
        _donations = donations;
        _nav = nav;
    }

    public ObservableCollection<DonationItemViewModel> Items { get; } = new();

    [ObservableProperty] private string totalCount = "0";
    [ObservableProperty] private string presencesCount = "0";
    [ObservableProperty] private string levelName = "-";
    [ObservableProperty] private string medalImage = "medal_none.png";
    [ObservableProperty] private string nextText = string.Empty;
    [ObservableProperty] private bool canDonate = true;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showEmpty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    [RelayCommand]
    private async Task LoadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        ShowEmpty = false;
        try
        {
            var donations = await _donations.GetDonationsAsync(user.Phone);
            var stats = DonationStatsCalculator.Compute(donations, DateTime.Today);

            Items.Clear();
            foreach (var d in donations) Items.Add(new DonationItemViewModel(d));
            ShowEmpty = Items.Count == 0;

            TotalCount = stats.ValidatedCount.ToString();
            PresencesCount = stats.PresencesCount.ToString();
            LevelName = LevelPolicy.Label(stats.Level);
            MedalImage = UiAssets.Medal(stats.Level);
            CanDonate = stats.CanDonateNow;
            if (stats.CanDonateNow)
                NextText = "✓ Prochain don possible : maintenant. Indication seulement : la décision finale revient au centre de don.";
            else
            {
                var next = stats.NextPossible!.Value.Date;
                var days = Math.Max(1, (next - DateTime.Today).Days);
                NextText = $"▲ Prochain don possible le {Labels.DateText(next)} (dans {Labels.DaysText(days)}). Indication seulement.";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger ton historique. Vérifie ta connexion.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FindCentresAsync() =>
        await _nav.ShowMessageAsync("Centres de collecte", "L'écran 13 du cahier sera ajouté dans une prochaine étape.");
}

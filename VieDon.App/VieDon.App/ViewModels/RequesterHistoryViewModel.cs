using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Écran 25 : Historique des alertes passées (pourvues, expirées, annulées), avec filtre par statut.</summary>
public partial class RequesterHistoryViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IAlertService _alerts;
    private readonly INavigationService _nav;
    private List<RequesterAlertSummary> _all = new();
    private string _filter = "all";

    public RequesterHistoryViewModel(ISessionService session, IAlertService alerts, INavigationService nav)
    {
        _session = session;
        _alerts = alerts;
        _nav = nav;

        Filters = new ObservableCollection<ChipOption>
        {
            new("Toutes", "all", Select) { IsSelected = true },
            new("Pourvues", "filled", Select),
            new("Expirées", "expired", Select),
            new("Annulées", "cancelled", Select)
        };
    }

    public ObservableCollection<ChipOption> Filters { get; }
    public ObservableCollection<RequesterAlertItemViewModel> Items { get; } = new();

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showEmpty;
    [ObservableProperty] private string emptyText = string.Empty;
    [ObservableProperty] private string summaryText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    private void Select(ChipOption option)
    {
        foreach (var f in Filters) f.IsSelected = ReferenceEquals(f, option);
        _filter = option.Value;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Items.Clear();
        foreach (var s in _all.Where(Matches))
            Items.Add(new RequesterAlertItemViewModel(s, Open));

        ShowEmpty = Items.Count == 0;
        EmptyText = _all.Count == 0
            ? "Aucune alerte passée. Tes alertes terminées apparaîtront ici."
            : "Aucune alerte avec ce statut.";
    }

    private bool Matches(RequesterAlertSummary s) => _filter switch
    {
        "filled" => s.Alert.Status == AlertStatus.Filled,
        "expired" => s.Alert.Status == AlertStatus.Expired,
        "cancelled" => s.Alert.Status == AlertStatus.Cancelled,
        _ => true
    };

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
            var items = await _alerts.GetRequesterAlertsAsync(user);
            // L'historique ne contient que les alertes terminées ; celles en cours sont sur l'Accueil.
            _all = items.Where(i => i.Alert.Status != AlertStatus.Active).ToList();
            var filled = _all.Count(i => i.Alert.Status == AlertStatus.Filled);
            SummaryText = _all.Count == 0
                ? string.Empty
                : $"{_all.Count} alertes terminées, dont {filled} pourvues.";
            ApplyFilter();
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger l'historique. Vérifie ta connexion.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Open(RequesterAlertItemViewModel item) =>
        _ = _nav.ShowMessageAsync("Suivi d'une alerte", "L'écran 23 (suivi détaillé d'une alerte) sera ajouté dans une prochaine étape.");
}

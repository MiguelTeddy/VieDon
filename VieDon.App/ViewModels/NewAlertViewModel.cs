using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Compatibility;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 22 : Nouvelle alerte. RG2 (aperçu des groupes ciblés), RG3 (Critique réservé), RG17, RG18.</summary>
public partial class NewAlertViewModel : ObservableObject
{
    private readonly IAlertService _alerts;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public NewAlertViewModel(IAlertService alerts, ISessionService session, INavigationService nav)
    {
        _alerts = alerts;
        _session = session;
        _nav = nav;

        GroupOptions = new ObservableCollection<ChipOption>(
            new[] { "A", "B", "AB", "O" }.Select(g => new ChipOption(g, g, SelectGroup)));
        RhesusOptions = new ObservableCollection<ChipOption>(new[]
        {
            new ChipOption("+", "pos", SelectRhesus),
            new ChipOption("-", "neg", SelectRhesus)
        });

        var user = _session.CurrentUser;
        var allowed = user is null ? Array.Empty<UrgencyLevel>() : AlertPolicy.AllowedLevels(user);
        UrgencyOptions = new ObservableCollection<ChipOption>(new[]
        {
            new ChipOption("Normale", "normal", SelectUrgency) { IsEnabled = allowed.Contains(UrgencyLevel.Normal), LockedHint = "compte à vérifier" },
            new ChipOption("Urgente", "urgent", SelectUrgency) { IsEnabled = allowed.Contains(UrgencyLevel.Urgent), LockedHint = "compte à vérifier" },
            new ChipOption("Critique", "critical", SelectUrgency) { IsEnabled = allowed.Contains(UrgencyLevel.Critical), LockedHint = "réservé aux établissements vérifiés" }
        });

        UpdatePreview();
    }

    public ObservableCollection<ChipOption> GroupOptions { get; }
    public ObservableCollection<ChipOption> RhesusOptions { get; }
    public ObservableCollection<ChipOption> UrgencyOptions { get; }

    // TODO : remplacer par l'annuaire réel des établissements de santé et centres de collecte (RG17).
    public ObservableCollection<string> PlaceNames { get; } = new()
    {
        "Hôpital Central", "Clinique du Centre", "Centre de santé Exemple", "Banque de sang Exemple"
    };

    [ObservableProperty] private BloodGroup? selectedGroup;
    [ObservableProperty] private Rhesus? selectedRhesus;
    [ObservableProperty] private UrgencyLevel? selectedUrgency;
    [ObservableProperty] private bool includeCompatible;
    [ObservableProperty] private int pouches = 1;
    [ObservableProperty] private int placeIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RadiusText))]
    private double radiusKm = 10;

    public int RadiusRounded => (int)Math.Round(RadiusKm);
    public string RadiusText => $"Rayon : {RadiusRounded} km";

    [ObservableProperty] private string previewText = string.Empty;

    [ObservableProperty] private string groupError = string.Empty;
    [ObservableProperty] private string rhesusError = string.Empty;
    [ObservableProperty] private string placeError = string.Empty;
    [ObservableProperty] private string urgencyError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGeneralError))]
    [NotifyPropertyChangedFor(nameof(GeneralErrorDisplay))]
    private string generalError = string.Empty;

    public bool HasGeneralError => !string.IsNullOrEmpty(GeneralError);
    public string GeneralErrorDisplay => $"Erreur : {GeneralError}";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool isBusy;

    partial void OnIncludeCompatibleChanged(bool value) => UpdatePreview();
    partial void OnSelectedGroupChanged(BloodGroup? value) => UpdatePreview();
    partial void OnSelectedRhesusChanged(Rhesus? value) => UpdatePreview();

    private void UpdatePreview()
    {
        if (SelectedGroup is null || SelectedRhesus is null)
        {
            PreviewText = "Choisis le groupe du patient pour voir les groupes de donneurs ciblés.";
            return;
        }
        var groups = CompatibilityTable.TargetedGroups(SelectedGroup.Value, SelectedRhesus.Value, IncludeCompatible)
            .Select(g => Labels.GroupLabel(g.Group, g.Rhesus));
        PreviewText = $"Groupes de donneurs ciblés : {string.Join(", ", groups)}";
    }

    private void SelectGroup(ChipOption option)
    {
        foreach (var o in GroupOptions) o.IsSelected = o == option;
        SelectedGroup = Enum.Parse<BloodGroup>(option.Value);
        GroupError = string.Empty;
    }

    private void SelectRhesus(ChipOption option)
    {
        foreach (var o in RhesusOptions) o.IsSelected = o == option;
        SelectedRhesus = option.Value == "pos" ? Rhesus.Positive : Rhesus.Negative;
        RhesusError = string.Empty;
    }

    private void SelectUrgency(ChipOption option)
    {
        if (!option.IsEnabled) return;
        foreach (var o in UrgencyOptions) o.IsSelected = o == option;
        SelectedUrgency = option.Value switch
        {
            "critical" => UrgencyLevel.Critical,
            "urgent" => UrgencyLevel.Urgent,
            _ => UrgencyLevel.Normal
        };
        UrgencyError = string.Empty;
    }

    [RelayCommand]
    private void IncreasePouches() { if (Pouches < 20) Pouches++; }

    [RelayCommand]
    private void DecreasePouches() { if (Pouches > 1) Pouches--; }

    private bool CanSend() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        GroupError = RhesusError = PlaceError = UrgencyError = GeneralError = string.Empty;
        var user = _session.CurrentUser;
        var errors = new List<string>();

        if (user is null || !AlertPolicy.CanLaunch(user))
        {
            GeneralError = "Ton compte doit être vérifié avant de lancer une alerte.";
            SemanticScreenReader.Announce($"Erreur : {GeneralError}");
            return;
        }
        if (SelectedGroup is null) { GroupError = "Choisis le groupe du patient."; errors.Add(GroupError); }
        if (SelectedRhesus is null) { RhesusError = "Choisis le rhésus du patient."; errors.Add(RhesusError); }
        if (PlaceIndex < 0) { PlaceError = "Choisis un établissement de santé."; errors.Add(PlaceError); }
        if (SelectedUrgency is null) { UrgencyError = "Choisis le niveau d'urgence."; errors.Add(UrgencyError); }
        else if (!AlertPolicy.IsLevelAllowed(user, SelectedUrgency.Value))
        { UrgencyError = "Ce niveau n'est pas disponible pour ton compte."; errors.Add(UrgencyError); }

        if (errors.Count > 0)
        {
            SemanticScreenReader.Announce($"Erreur : {errors[0]}");
            return;
        }

        IsBusy = true;
        try
        {
            var request = new NewAlertRequest(SelectedGroup!.Value, SelectedRhesus!.Value, Pouches,
                PlaceNames[PlaceIndex], SelectedUrgency!.Value, RadiusRounded, IncludeCompatible);
            await _alerts.CreateAlertAsync(request, user);
            await _nav.ShowMessageAsync("Alerte envoyée",
                "Les donneurs ciblés vont être notifiés. Tu peux suivre les réponses depuis ton accueil.");
            await _nav.GoToAsync(Routes.RequesterHome);
        }
        catch (Exception)
        {
            GeneralError = "Connexion interrompue. Ton alerte n'a pas été envoyée. Réessaie.";
            SemanticScreenReader.Announce($"Erreur : {GeneralError}");
        }
        finally { IsBusy = false; }
    }
}

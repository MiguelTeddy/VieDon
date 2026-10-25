using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.ViewModels;

/// <summary>Écran 17 : Détail d'un avantage partenaire (conditions, niveau requis, validité, bouton Utiliser).</summary>
public partial class RewardDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly ISessionService _session;
    private readonly IDonationService _donations;
    private readonly INavigationService _nav;
    private Guid _id;

    public RewardDetailViewModel(ISessionService session, IDonationService donations, INavigationService nav)
    {
        _session = session;
        _donations = donations;
        _nav = nav;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && Guid.TryParse(value?.ToString(), out var id))
        {
            _id = id;
            _ = LoadAsync();
        }
    }

    [ObservableProperty] private bool isLoaded;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string partner = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string conditions = string.Empty;
    [ObservableProperty] private string requiredText = string.Empty;
    [ObservableProperty] private string validityText = string.Empty;
    [ObservableProperty] private string statusText = string.Empty;
    [ObservableProperty] private string lockedHint = string.Empty;

    [ObservableProperty] private bool isAvailable;
    [ObservableProperty] private bool isUsed;
    [ObservableProperty] private bool isLocked;
    [ObservableProperty] private bool isExpired;

    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UseCommand))] private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";
    public bool HasCode => !string.IsNullOrEmpty(Code);
    public string CodeHint => "À présenter chez le partenaire. Ce code est personnel et n'a aucune valeur monétaire.";

    partial void OnCodeChanged(string value) => OnPropertyChanged(nameof(HasCode));

    private async Task LoadAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var item = await _donations.GetRewardAsync(user.Phone, _id);
            if (item is null) { ErrorMessage = "Avantage introuvable."; return; }
            Apply(item);
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de charger l'avantage. Vérifie ta connexion.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Apply(RewardItem item)
    {
        var r = item.Reward;
        Title = r.Title;
        Partner = r.Partner;
        Description = r.Description;
        Conditions = r.Conditions;
        RequiredText = $"Niveau requis : {LevelPolicy.Label(r.RequiredLevel)} ({LevelPolicy.Threshold(r.RequiredLevel)} dons validés)";
        ValidityText = $"Valable jusqu'au {Labels.DateText(r.ValidUntil)}";

        IsAvailable = item.Status == RewardStatus.Available;
        IsUsed = item.Status == RewardStatus.Used;
        IsLocked = item.Status == RewardStatus.Locked;
        IsExpired = item.Status == RewardStatus.Expired;
        Code = item.UseCode ?? string.Empty;

        StatusText = item.Status switch
        {
            RewardStatus.Available => "✓ Disponible",
            RewardStatus.Used => "✓ Déjà utilisé",
            RewardStatus.Locked => $"▲ Verrouillé : niveau {LevelPolicy.Label(r.RequiredLevel)} requis",
            _ => "✕ Expiré"
        };
        LockedHint = $"Atteins le niveau {LevelPolicy.Label(r.RequiredLevel)} ({LevelPolicy.Threshold(r.RequiredLevel)} dons validés) pour débloquer cet avantage.";
        IsLoaded = true;
    }

    private bool CanUse() => !IsBusy && IsAvailable;

    [RelayCommand(CanExecute = nameof(CanUse))]
    private async Task UseAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;

        var yes = await _nav.ConfirmAsync("Utiliser cet avantage ?",
            "Un code personnel va être généré. L'avantage ne pourra être utilisé qu'une fois.", "Utiliser", "Annuler");
        if (!yes) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var item = await _donations.UseRewardAsync(user.Phone, _id);
            if (item is not null)
            {
                Apply(item);
                SemanticScreenReader.Announce($"Avantage utilisé. Ton code : {Code}");
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Connexion interrompue. Réessaie dans un instant.";
            SemanticScreenReader.Announce($"Erreur : {ErrorMessage}");
        }
        finally
        {
            IsBusy = false;
            UseCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private async Task BackAsync() => await _nav.GoToAsync("..");
}

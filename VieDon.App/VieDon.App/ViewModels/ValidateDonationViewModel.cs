using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;

namespace VieDon.App.ViewModels;

/// <summary>
/// Écran 24 : Valider un don. L'établissement saisit le code présenté par le donneur (le scan de QR code viendra plus tard).
/// RG10 : un don n'entre dans l'historique, le niveau et les avantages qu'après cette validation. RG8 : le groupe devient Confirmé.
/// </summary>
public partial class ValidateDonationViewModel : ObservableObject
{
    private readonly IAlertService _alerts;
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public ValidateDonationViewModel(IAlertService alerts, IAuthService auth, ISessionService session, INavigationService nav)
    {
        _alerts = alerts;
        _auth = auth;
        _session = session;
        _nav = nav;
    }

    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private string codeError = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateCommand))] private bool isBusy;

    // Résultat : succès (✓) ou avertissement (▲ / ✕), toujours en symbole + mot.
    [ObservableProperty] private bool showSuccess;
    [ObservableProperty] private bool showWarning;
    [ObservableProperty] private string resultTitle = string.Empty;
    [ObservableProperty] private string resultDetail = string.Empty;

    public bool IsDevMode =>
#if DEBUG
        true;
#else
        false;
#endif

    public string DevHint =>
        "Pour tester : connecte-toi avec un compte donneur (par exemple Nadia Bello), accepte l'alerte Critique O+ de l'Hôpital Central, note son code VD-xxxx, puis reviens ici avec le compte Hôpital Central.";

    public string RulesText =>
        "Le don n'est comptabilisé dans l'historique, le niveau et les avantages du donneur qu'après ta validation. Le groupe sanguin reste contrôlé par ton établissement.";

    private bool CanValidate() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanValidate))]
    private async Task ValidateAsync()
    {
        CodeError = string.Empty;
        ShowSuccess = false;
        ShowWarning = false;

        var user = _session.CurrentUser;
        if (user is null) return;

        if (string.IsNullOrWhiteSpace(Code))
        {
            CodeError = "saisis le code présenté par le donneur (exemple : VD-1234).";
            SemanticScreenReader.Announce($"Erreur : {CodeError}");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _alerts.ValidateDonationAsync(Code, user);
            switch (result.Status)
            {
                case ValidationStatus.Validated:
                    // RG8 : un don validé confirme le groupe du donneur.
                    if (result.DonorPhone is not null) await _auth.ConfirmGroupAsync(result.DonorPhone);
                    ResultTitle = "✓ Don validé";
                    ResultDetail = $"{result.DonorName}, groupe {result.GroupLabel}, à {result.PlaceName}. Le don est ajouté à son historique, son groupe est confirmé et son niveau est mis à jour.";
                    ShowSuccess = true;
                    Code = string.Empty;
                    break;

                case ValidationStatus.AlreadyValidated:
                    ResultTitle = "▲ Code déjà utilisé";
                    ResultDetail = $"Le don de {result.DonorName} a déjà été validé. Rien n'a été compté en double.";
                    ShowWarning = true;
                    break;

                case ValidationStatus.UnknownCode:
                    CodeError = "code inconnu. Vérifie le code avec le donneur.";
                    break;

                default:
                    ResultTitle = "✕ Action non autorisée";
                    ResultDetail = "Seul un établissement dont le compte est vérifié peut valider un don.";
                    ShowWarning = true;
                    break;
            }

            SemanticScreenReader.Announce(ShowSuccess || ShowWarning ? $"{ResultTitle}. {ResultDetail}" : $"Erreur : {CodeError}");
        }
        catch (Exception)
        {
            ResultTitle = "✕ Connexion interrompue";
            ResultDetail = "Réessaie dans un instant.";
            ShowWarning = true;
            SemanticScreenReader.Announce($"{ResultTitle}. {ResultDetail}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync() => await _nav.GoToAsync("..");
}

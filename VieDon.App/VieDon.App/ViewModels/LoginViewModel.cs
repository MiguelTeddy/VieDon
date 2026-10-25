using System.Collections.ObjectModel;
using VieDon.Core.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.Core.Models;
using VieDon.App.Services;

namespace VieDon.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public LoginViewModel(IAuthService auth, ISessionService session, INavigationService nav)
    {
        _auth = auth;
        _session = session;
        _nav = nav;

        DemoAccounts = new ObservableCollection<DemoAccountItem>(
            DemoData.Accounts.Select(a => new DemoAccountItem(a, UseDemo)));
    }

    // Comptes d'exemple : visibles en développement (Debug) seulement.
    public ObservableCollection<DemoAccountItem> DemoAccounts { get; }

    public bool IsDevMode =>
#if DEBUG
        true;
#else
        false;
#endif

    public string DemoPasswordText => $"Mot de passe des comptes d'exemple : {DemoData.Password}. Administrateur : {DemoData.AdminLogin} / {DemoData.AdminPassword}";

    private void UseDemo(DemoAccount account)
    {
        Identifier = account.Phone;
        Password = string.Equals(account.Phone, DemoData.AdminLogin, StringComparison.OrdinalIgnoreCase)
            ? DemoData.AdminPassword
            : DemoData.Password;
        if (LoginCommand.CanExecute(null)) LoginCommand.Execute(null);
    }

    public string SpaceTitle =>
        (_session.SelectedSpace ?? UserSpace.Donor) == UserSpace.Donor ? "Espace Donneur" : "Espace Demandeur";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string identifier = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorDisplay))]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorDisplay => $"Erreur : {ErrorMessage}";

    private bool CanLogin() =>
        !IsBusy && !string.IsNullOrWhiteSpace(Identifier) && !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

#if DEBUG
        // Administration de démonstration : n'existe pas dans la version finale.
        if (string.Equals(Identifier.Trim(), DemoData.AdminLogin, StringComparison.OrdinalIgnoreCase))
        {
            if (Password != DemoData.AdminPassword)
            {
                SetError("Identifiants incorrects. Vérifie ton identifiant et ton mot de passe.");
                return;
            }
            _session.Clear();
            await _nav.GoToAsync(Routes.Admin);
            return;
        }
#endif

        if (!Validators.IsPhoneOrEmail(Identifier))
        {
            SetError("Saisis un numéro de téléphone ou une adresse e-mail valide.");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _auth.LoginAsync(Identifier.Trim(), Password);
            switch (result.Status)
            {
                case LoginStatus.Success when result.Profile is not null:
                    _session.CurrentUser = result.Profile;
                    _session.SelectedSpace = result.Profile.Space;
                    if (result.Profile.Space == UserSpace.Donor)
                    {
                        await _nav.GoToAsync(Routes.DonorHome);
                    }
                    else
                    {
                        await _nav.GoToAsync(Routes.RequesterHome);
                        // Écran 20 : à chaque connexion tant que le compte n'est pas validé.
                        if (result.Profile.Verification != VerificationStatus.Verified)
                            await _nav.GoToAsync(Routes.RequesterVerification);
                    }
                    break;
                case LoginStatus.AccountSuspended:
                    SetError("Compte suspendu. Contacte le support.");
                    break;
                case LoginStatus.NetworkError:
                    SetError("Connexion interrompue. Réessaie dans un instant.");
                    break;
                default:
                    SetError("Identifiants incorrects. Vérifie ton identifiant et ton mot de passe.");
                    break;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync() =>
        await _nav.ShowMessageAsync("Mot de passe oublié", "Cet écran (écran 3 du cahier) sera ajouté dans une prochaine étape.");

    [RelayCommand]
    private async Task CreateAccountAsync()
    {
        if ((_session.SelectedSpace ?? UserSpace.Donor) == UserSpace.Donor)
            await _nav.GoToAsync(Routes.DonorSignup);
        else
            await _nav.GoToAsync(Routes.RequesterSignup);
    }

    [RelayCommand]
    private async Task ChangeProfileAsync() => await _nav.GoToAsync("..");

    private void SetError(string message)
    {
        ErrorMessage = message;
        SemanticScreenReader.Announce($"Erreur : {message}"); // lecteur d'écran
    }
}

using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Touche « Suivant » : passe au mot de passe ; touche « Terminé » : envoie le formulaire.
    private void OnIdentifierCompleted(object? sender, EventArgs e) => PasswordEntry.FocusInput();

    private void OnPasswordCompleted(object? sender, EventArgs e)
    {
        if (_viewModel.LoginCommand.CanExecute(null))
            _viewModel.LoginCommand.Execute(null);
    }
}

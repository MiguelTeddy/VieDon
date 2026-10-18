using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class DonorSignupPage : ContentPage
{
    public DonorSignupPage(DonorSignupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Touche « Suivant » : enchaîne les champs dans l'ordre de lecture.
    private void OnFullNameCompleted(object? sender, EventArgs e) => PhoneEntry.FocusInput();
    private void OnPhoneCompleted(object? sender, EventArgs e) => PasswordEntry.FocusInput();
    private void OnPasswordCompleted(object? sender, EventArgs e) => ConfirmEntry.FocusInput();
}

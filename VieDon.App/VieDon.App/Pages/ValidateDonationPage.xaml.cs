using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class ValidateDonationPage : ContentPage
{
    public ValidateDonationPage(ValidateDonationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

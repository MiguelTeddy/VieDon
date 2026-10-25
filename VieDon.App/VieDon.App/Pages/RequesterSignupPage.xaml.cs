using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RequesterSignupPage : ContentPage
{
    public RequesterSignupPage(RequesterSignupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

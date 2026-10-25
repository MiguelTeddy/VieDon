using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class NewAlertPage : ContentPage
{
    public NewAlertPage(NewAlertViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

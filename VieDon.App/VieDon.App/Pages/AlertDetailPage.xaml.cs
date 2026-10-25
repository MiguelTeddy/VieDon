using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class AlertDetailPage : ContentPage
{
    public AlertDetailPage(AlertDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

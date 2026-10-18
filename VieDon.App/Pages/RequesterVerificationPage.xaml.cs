using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RequesterVerificationPage : ContentPage
{
    private readonly RequesterVerificationViewModel _viewModel;

    public RequesterVerificationPage(RequesterVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshCommand.ExecuteAsync(null);
    }
}

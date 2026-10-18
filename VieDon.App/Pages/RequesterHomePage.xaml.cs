using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RequesterHomePage : ContentPage
{
    private readonly RequesterHomeViewModel _viewModel;

    public RequesterHomePage(RequesterHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}

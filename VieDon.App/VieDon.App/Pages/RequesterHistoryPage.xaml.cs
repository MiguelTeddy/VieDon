using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RequesterHistoryPage : ContentPage
{
    private readonly RequesterHistoryViewModel _viewModel;

    public RequesterHistoryPage(RequesterHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}

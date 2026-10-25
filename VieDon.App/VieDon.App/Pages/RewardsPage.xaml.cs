using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RewardsPage : ContentPage
{
    private readonly RewardsViewModel _viewModel;

    public RewardsPage(RewardsViewModel viewModel)
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

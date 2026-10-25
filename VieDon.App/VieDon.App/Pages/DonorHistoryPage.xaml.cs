using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class DonorHistoryPage : ContentPage
{
    private readonly DonorHistoryViewModel _viewModel;

    public DonorHistoryPage(DonorHistoryViewModel viewModel)
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

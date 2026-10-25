using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class DonorHomePage : ContentPage
{
    private readonly DonorHomeViewModel _viewModel;

    public DonorHomePage(DonorHomeViewModel viewModel)
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

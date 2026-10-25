using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class DonorProfilePage : ContentPage
{
    private readonly DonorProfileViewModel _viewModel;

    public DonorProfilePage(DonorProfileViewModel viewModel)
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

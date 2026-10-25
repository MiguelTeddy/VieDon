using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RequesterProfilePage : ContentPage
{
    private readonly RequesterProfileViewModel _viewModel;

    public RequesterProfilePage(RequesterProfileViewModel viewModel)
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

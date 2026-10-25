using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _viewModel;

    public AdminPage(AdminViewModel viewModel)
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

using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class RewardDetailPage : ContentPage
{
    public RewardDetailPage(RewardDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

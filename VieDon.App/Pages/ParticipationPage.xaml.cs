using VieDon.App.ViewModels;

namespace VieDon.App.Pages;

public partial class ParticipationPage : ContentPage
{
    public ParticipationPage(ParticipationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

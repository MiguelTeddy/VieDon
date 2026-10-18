using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.Core.Models;
using VieDon.App.Services;

namespace VieDon.App.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public WelcomeViewModel(ISessionService session, INavigationService nav)
    {
        _session = session;
        _nav = nav;
    }

    // TODO : brancher la localisation (fichiers .resx fr / en). Pour l'instant seul l'état visuel change.
    [ObservableProperty]
    private string selectedLanguage = "fr";

    [RelayCommand]
    private void SetLanguage(string code) => SelectedLanguage = code;

    [RelayCommand]
    private async Task ChooseDonorAsync()
    {
        _session.SelectedSpace = UserSpace.Donor;
        await _nav.GoToAsync(Routes.DonorSignup);
    }

    [RelayCommand]
    private async Task ChooseRequesterAsync()
    {
        _session.SelectedSpace = UserSpace.Requester;
        await _nav.GoToAsync(Routes.RequesterSignup);
    }

    [RelayCommand]
    private async Task HaveAccountAsync() => await _nav.GoToAsync(Routes.Login);
}

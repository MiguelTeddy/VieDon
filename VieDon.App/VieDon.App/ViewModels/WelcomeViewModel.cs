using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.Core.Models;
using VieDon.App.Localization;
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

    [ObservableProperty]
    private string selectedLanguage = Loc.Language;

    [RelayCommand]
    private void SetLanguage(string code)
    {
        if (code == SelectedLanguage) return;
        SelectedLanguage = code;
        Loc.SetLanguage(code);

        // On reconstruit l'application pour que tous les écrans reprennent la langue choisie.
        if (Application.Current is not null)
            Application.Current.MainPage = new AppShell();
    }

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

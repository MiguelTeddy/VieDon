using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core.Models;

namespace VieDon.App.ViewModels;

/// <summary>Écran 20 : Vérification du compte demandeur (RG15 : justificatif vérifié par une personne).</summary>
public partial class RequesterVerificationViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public RequesterVerificationViewModel(IAuthService auth, ISessionService session, INavigationService nav)
    {
        _auth = auth;
        _session = session;
        _nav = nav;
    }

    [ObservableProperty] private bool isPending = true;
    [ObservableProperty] private bool isVerified;
    [ObservableProperty] private bool isRejected;
    [ObservableProperty] private string statusTitle = "Vérification en cours";
    [ObservableProperty] private string statusDetail = string.Empty;

    public bool IsDevMode =>
#if DEBUG
        true;
#else
        false;
#endif

    private void Apply(UserProfile profile)
    {
        IsPending = profile.Verification == VerificationStatus.Pending;
        IsVerified = profile.Verification == VerificationStatus.Verified;
        IsRejected = profile.Verification == VerificationStatus.Rejected;

        (StatusTitle, StatusDetail) = profile.Verification switch
        {
            VerificationStatus.Verified => ("Compte vérifié",
                "Ton compte est validé. Tu peux lancer des alertes."),
            VerificationStatus.Rejected => ("Justificatif refusé",
                "Ton justificatif n'a pas pu être validé. Contacte le support pour en envoyer un nouveau."),
            _ => ("Vérification en cours",
                "Une personne vérifie ton justificatif d'autorisation. Tu pourras lancer des alertes dès que ton compte sera validé. Le délai de vérification sera précisé après le pilote.")
        };
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var user = _session.CurrentUser;
        if (user is null) return;
        var fresh = await _auth.GetProfileAsync(user.Phone) ?? user;
        _session.CurrentUser = fresh;
        Apply(fresh);
    }

    [RelayCommand]
    private async Task ContinueAsync() => await _nav.GoToAsync("..");

    [RelayCommand]
    private async Task SupportAsync() =>
        await _nav.ShowMessageAsync("Support", "Le contact du support sera ajouté avec l'écran 5 (Aide, conditions et confidentialité).");

    // ----- Développement uniquement : remplace la décision d'un vérificateur humain.
    [RelayCommand]
    private async Task SimulateApproveAsync()
    {
        if (_session.CurrentUser is null) return;
        await _auth.SimulateVerificationAsync(_session.CurrentUser.Phone, true);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SimulateRejectAsync()
    {
        if (_session.CurrentUser is null) return;
        await _auth.SimulateVerificationAsync(_session.CurrentUser.Phone, false);
        await RefreshAsync();
    }
}

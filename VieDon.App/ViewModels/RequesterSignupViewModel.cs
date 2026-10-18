using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;
using VieDon.Core;
using VieDon.Core.Formatting;
using VieDon.Core.Models;
using VieDon.Core.Validation;

namespace VieDon.App.ViewModels;

/// <summary>Écran 19 : Inscription demandeur (individu ou établissement de santé).</summary>
public partial class RequesterSignupViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public RequesterSignupViewModel(IAuthService auth, ISessionService session, INavigationService nav)
    {
        _auth = auth;
        _session = session;
        _nav = nav;

        KindOptions = new ObservableCollection<ChipOption>
        {
            new ChipOption("Individu (proche d'un patient)", "ind", SelectKind),
            new ChipOption("Établissement", "org", SelectKind)
        };
        KindOptions[0].IsSelected = true;

        OrgTypeLabels = new ObservableCollection<string>(
            Enum.GetValues<EstablishmentType>().Select(Labels.EstablishmentTypeLabel));
    }

    public ObservableCollection<ChipOption> KindOptions { get; }
    public ObservableCollection<string> OrgTypeLabels { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEstablishment))]
    [NotifyPropertyChangedFor(nameof(NameLabel))]
    private RequesterKind kind = RequesterKind.Individual;

    public bool IsEstablishment => Kind == RequesterKind.Establishment;
    public string NameLabel => IsEstablishment ? "Nom du responsable" : "Nom complet";

    // Établissement
    [ObservableProperty] private int orgTypeIndex = -1;
    [ObservableProperty] private string organizationName = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string professionalEmail = string.Empty;
    [ObservableProperty] private string justificatifName = string.Empty;

    // Commun
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private string smsCode = string.Empty;
    [ObservableProperty] private bool acceptTerms;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SendCodeText))]
    [NotifyPropertyChangedFor(nameof(ShowCodeField))]
    private bool isCodeSent;

    public string SendCodeText => IsCodeSent ? "Renvoyer le code" : "Recevoir le code par SMS";
    public bool ShowCodeField => IsCodeSent;

    // Erreurs par champ
    [ObservableProperty] private string orgTypeError = string.Empty;
    [ObservableProperty] private string organizationNameError = string.Empty;
    [ObservableProperty] private string addressError = string.Empty;
    [ObservableProperty] private string professionalEmailError = string.Empty;
    [ObservableProperty] private string justificatifError = string.Empty;
    [ObservableProperty] private string fullNameError = string.Empty;
    [ObservableProperty] private string phoneError = string.Empty;
    [ObservableProperty] private string passwordError = string.Empty;
    [ObservableProperty] private string confirmPasswordError = string.Empty;
    [ObservableProperty] private string smsCodeError = string.Empty;
    [ObservableProperty] private string termsError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGeneralError))]
    [NotifyPropertyChangedFor(nameof(GeneralErrorDisplay))]
    private string generalError = string.Empty;

    public bool HasGeneralError => !string.IsNullOrEmpty(GeneralError);
    public string GeneralErrorDisplay => $"Erreur : {GeneralError}";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCodeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CreateAccountCommand))]
    private bool isBusy;

    private bool CanAct() => !IsBusy;

    private void SelectKind(ChipOption option)
    {
        foreach (var o in KindOptions) o.IsSelected = o == option;
        Kind = option.Value == "org" ? RequesterKind.Establishment : RequesterKind.Individual;
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task SendCodeAsync()
    {
        PhoneError = string.Empty;
        if (!Validators.IsValidPhone(Phone))
        {
            PhoneError = "Saisis un numéro de téléphone valide (8 à 15 chiffres).";
            SemanticScreenReader.Announce($"Erreur : {PhoneError}");
            return;
        }
        IsBusy = true;
        try
        {
            IsCodeSent = await _auth.SendSmsCodeAsync(Phone);
            if (!IsCodeSent) PhoneError = "Impossible d'envoyer le code. Réessaie.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PickJustificatifAsync()
    {
        try
        {
            var types = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/pdf", "image/*" } },
                { DevicePlatform.iOS, new[] { "com.adobe.pdf", "public.image" } },
                { DevicePlatform.MacCatalyst, new[] { "com.adobe.pdf", "public.image" } },
                { DevicePlatform.WinUI, new[] { ".pdf", ".jpg", ".jpeg", ".png" } }
            });
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choisir le justificatif d'autorisation",
                FileTypes = types
            });
            if (result is not null)
            {
                JustificatifName = result.FileName;
                JustificatifError = string.Empty;
            }
        }
        catch (Exception)
        {
            JustificatifError = "Impossible de lire le fichier. Réessaie avec un PDF ou une photo.";
        }
    }

    [RelayCommand]
    private async Task OpenTermsAsync() =>
        await _nav.ShowMessageAsync("Conditions et confidentialité", "L'écran 5 du cahier sera ajouté dans une prochaine étape.");

    private bool Validate()
    {
        OrgTypeError = OrganizationNameError = AddressError = ProfessionalEmailError = JustificatifError = string.Empty;
        FullNameError = PhoneError = PasswordError = ConfirmPasswordError = SmsCodeError = TermsError = GeneralError = string.Empty;
        var errors = new List<string>();
        void Add(Action<string> set, string message) { set(message); errors.Add(message); }

        if (IsEstablishment)
        {
            if (OrgTypeIndex < 0) Add(v => OrgTypeError = v, "Choisis le type d'établissement.");
            if (string.IsNullOrWhiteSpace(OrganizationName)) Add(v => OrganizationNameError = v, "Saisis le nom de l'établissement.");
            if (string.IsNullOrWhiteSpace(Address)) Add(v => AddressError = v, "Saisis l'adresse de l'établissement.");
            if (!Validators.IsValidEmail(ProfessionalEmail)) Add(v => ProfessionalEmailError = v, "Saisis une adresse e-mail professionnelle valide.");
            if (string.IsNullOrWhiteSpace(JustificatifName)) Add(v => JustificatifError = v, "Ajoute ton justificatif d'autorisation (PDF ou photo).");
        }
        if (string.IsNullOrWhiteSpace(FullName)) Add(v => FullNameError = v, $"Saisis : {NameLabel.ToLowerInvariant()}.");
        if (!Validators.IsValidPhone(Phone)) Add(v => PhoneError = v, "Saisis un numéro de téléphone valide (8 à 15 chiffres).");
        if (Password.Length < AppConstants.PasswordMinLength)
            Add(v => PasswordError = v, $"Mot de passe : {AppConstants.PasswordMinLength} caractères minimum.");
        if (ConfirmPassword != Password) Add(v => ConfirmPasswordError = v, "Les mots de passe sont différents.");
        if (!IsCodeSent) Add(v => SmsCodeError = v, "Demande d'abord le code reçu par SMS.");
        else if (string.IsNullOrWhiteSpace(SmsCode)) Add(v => SmsCodeError = v, "Saisis le code reçu par SMS.");
        if (!AcceptTerms) Add(v => TermsError = v, "Accepte les conditions et le traitement de tes données pour continuer.");

        if (errors.Count > 0) SemanticScreenReader.Announce($"Erreur : {errors[0]}");
        return errors.Count == 0;
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task CreateAccountAsync()
    {
        if (!Validate()) return;
        IsBusy = true;
        try
        {
            if (!await _auth.VerifySmsCodeAsync(Phone, SmsCode.Trim()))
            {
                SmsCodeError = "Code incorrect ou expiré. Tu peux demander un nouveau code.";
                SemanticScreenReader.Announce($"Erreur : {SmsCodeError}");
                return;
            }

            var orgType = IsEstablishment ? Enum.GetValues<EstablishmentType>()[OrgTypeIndex] : (EstablishmentType?)null;
            var registration = new RequesterRegistration(Kind, FullName, Phone, Password, orgType,
                IsEstablishment ? OrganizationName : null, IsEstablishment ? Address : null,
                IsEstablishment ? ProfessionalEmail : null, IsEstablishment ? JustificatifName : null);

            var result = await _auth.RegisterRequesterAsync(registration);
            switch (result.Status)
            {
                case RegistrationStatus.Success when result.Profile is not null:
                    _session.SelectedSpace = UserSpace.Requester;
                    _session.CurrentUser = result.Profile;
                    await _nav.GoToAsync(Routes.RequesterHome);
                    if (result.Profile.Verification != VerificationStatus.Verified)
                        await _nav.GoToAsync(Routes.RequesterVerification); // écran 20
                    break;
                case RegistrationStatus.PhoneAlreadyUsed:
                    PhoneError = "Ce numéro a déjà un compte. Connecte-toi à la place.";
                    SemanticScreenReader.Announce($"Erreur : {PhoneError}");
                    break;
                default:
                    GeneralError = "Connexion interrompue. Réessaie dans un instant.";
                    SemanticScreenReader.Announce($"Erreur : {GeneralError}");
                    break;
            }
        }
        finally { IsBusy = false; }
    }
}

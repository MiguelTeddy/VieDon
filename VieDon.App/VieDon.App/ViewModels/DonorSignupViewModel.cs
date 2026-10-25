using VieDon.Core;
using VieDon.Core.Validation;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VieDon.Core.Models;
using VieDon.App.Services;

namespace VieDon.App.ViewModels;

public partial class DonorSignupViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;
    private readonly INavigationService _nav;

    public DonorSignupViewModel(IAuthService auth, ISessionService session, INavigationService nav)
    {
        _auth = auth;
        _session = session;
        _nav = nav;

        // Le groupe puis le rhésus, en deux temps (O+ et O- sont différents).
        GroupOptions = new ObservableCollection<ChipOption>(
            new[] { "A", "B", "AB", "O" }.Select(g => new ChipOption(g, g, SelectGroup)));
        RhesusOptions = new ObservableCollection<ChipOption>(new[]
        {
            new ChipOption("+", "pos", SelectRhesus),
            new ChipOption("-", "neg", SelectRhesus),
            new ChipOption("Je ne sais pas", "unk", SelectRhesus)
        });

        BirthDate = DateTime.Today.AddYears(-AppConstants.MinimumDonorAge);
    }

    public ObservableCollection<ChipOption> GroupOptions { get; }
    public ObservableCollection<ChipOption> RhesusOptions { get; }

    // ----- Étapes
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep1))]
    [NotifyPropertyChangedFor(nameof(IsStep2))]
    [NotifyPropertyChangedFor(nameof(StepLabel))]
    [NotifyPropertyChangedFor(nameof(Progress))]
    private int step = 1;

    public bool IsStep1 => Step == 1;
    public bool IsStep2 => Step == 2;
    public string StepLabel => $"Étape {Step} sur 2";
    public double Progress => Step / 2.0;

    // ----- Étape 1
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private string smsCode = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SendCodeText))]
    [NotifyPropertyChangedFor(nameof(ShowCodeField))]
    private bool isCodeSent;

    public string SendCodeText => IsCodeSent ? "Renvoyer le code" : "Recevoir le code par SMS";
    public bool ShowCodeField => IsCodeSent;

    [ObservableProperty] private string fullNameError = string.Empty;
    [ObservableProperty] private string phoneError = string.Empty;
    [ObservableProperty] private string passwordError = string.Empty;
    [ObservableProperty] private string confirmPasswordError = string.Empty;
    [ObservableProperty] private string smsCodeError = string.Empty;

    // ----- Étape 2
    [ObservableProperty] private BloodGroup? selectedGroup;
    [ObservableProperty] private Rhesus? selectedRhesus;
    [ObservableProperty] private DateTime birthDate;
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private bool acceptTerms;

    [ObservableProperty] private string groupError = string.Empty;
    [ObservableProperty] private string rhesusError = string.Empty;
    [ObservableProperty] private string birthDateError = string.Empty;
    [ObservableProperty] private string cityError = string.Empty;
    [ObservableProperty] private string termsError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGeneralError))]
    [NotifyPropertyChangedFor(nameof(GeneralErrorDisplay))]
    private string generalError = string.Empty;

    public bool HasGeneralError => !string.IsNullOrEmpty(GeneralError);
    public string GeneralErrorDisplay => $"Erreur : {GeneralError}";

    // RG11 : « Je ne sais pas » reste dans le réseau, avec une invitation à se faire typer.
    [ObservableProperty]
    private bool isRhesusUnknown;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    [NotifyCanExecuteChangedFor(nameof(CreateAccountCommand))]
    [NotifyCanExecuteChangedFor(nameof(SendCodeCommand))]
    private bool isBusy;

    // ----- Pastilles
    private void SelectGroup(ChipOption option)
    {
        foreach (var o in GroupOptions) o.IsSelected = o == option;
        SelectedGroup = Enum.Parse<BloodGroup>(option.Value);
        GroupError = string.Empty;
    }

    private void SelectRhesus(ChipOption option)
    {
        foreach (var o in RhesusOptions) o.IsSelected = o == option;
        SelectedRhesus = option.Value switch
        {
            "pos" => Rhesus.Positive,
            "neg" => Rhesus.Negative,
            _ => Rhesus.Unknown
        };
        IsRhesusUnknown = SelectedRhesus == Rhesus.Unknown;
        RhesusError = string.Empty;
    }

    private bool CanAct() => !IsBusy;

    // ----- Étape 1 : code SMS
    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task SendCodeAsync()
    {
        PhoneError = string.Empty;
        if (!Validators.IsValidPhone(Phone))
        {
            Fail(v => PhoneError = v, "Saisis un numéro de téléphone valide (8 à 15 chiffres).");
            return;
        }

        IsBusy = true;
        try
        {
            IsCodeSent = await _auth.SendSmsCodeAsync(Phone);
            if (!IsCodeSent) Fail(v => PhoneError = v, "Impossible d'envoyer le code. Réessaie.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task ContinueAsync()
    {
        if (!ValidateStep1()) return;

        IsBusy = true;
        try
        {
            var ok = await _auth.VerifySmsCodeAsync(Phone, SmsCode.Trim());
            if (!ok)
            {
                Fail(v => SmsCodeError = v, "Code incorrect ou expiré. Tu peux demander un nouveau code.");
                return;
            }
            Step = 2;
            SemanticScreenReader.Announce("Étape 2 sur 2");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Back()
    {
        if (Step == 2) Step = 1;
    }

    // ----- Étape 2 : création du compte
    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task CreateAccountAsync()
    {
        if (!ValidateStep2()) return;

        IsBusy = true;
        try
        {
            // Si le rhésus est inconnu, on considère que le donneur ne connaît pas son groupe (RG11).
            var knowsGroup = SelectedRhesus != Rhesus.Unknown;
            var registration = new DonorRegistration(
                FullName, Phone, Password,
                knowsGroup ? SelectedGroup : null,
                knowsGroup ? SelectedRhesus : Rhesus.Unknown,
                City, BirthDate);

            var result = await _auth.RegisterDonorAsync(registration);
            switch (result.Status)
            {
                case RegistrationStatus.Success when result.Profile is not null:
                    _session.CurrentUser = result.Profile;
                    await _nav.GoToAsync(Routes.DonorHome);
                    break;
                case RegistrationStatus.PhoneAlreadyUsed:
                    Step = 1;
                    Fail(v => PhoneError = v, "Ce numéro a déjà un compte. Connecte-toi à la place.");
                    break;
                default:
                    Fail(v => GeneralError = v, "Connexion interrompue. Réessaie dans un instant.");
                    break;
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task OpenTermsAsync() =>
        await _nav.ShowMessageAsync("Conditions et confidentialité", "L'écran 5 du cahier (conditions, confidentialité, aide) sera ajouté dans une prochaine étape.");

    // ----- Validation
    private bool ValidateStep1()
    {
        FullNameError = PhoneError = PasswordError = ConfirmPasswordError = SmsCodeError = string.Empty;
        var first = string.Empty;

        if (string.IsNullOrWhiteSpace(FullName)) { FullNameError = "Saisis ton nom complet."; first = FullNameError; }
        if (!Validators.IsValidPhone(Phone)) { PhoneError = "Saisis un numéro de téléphone valide (8 à 15 chiffres)."; first = first.Length > 0 ? first : PhoneError; }
        if (Password.Length < AppConstants.PasswordMinLength)
        { PasswordError = $"Mot de passe : {AppConstants.PasswordMinLength} caractères minimum."; first = first.Length > 0 ? first : PasswordError; }
        if (ConfirmPassword != Password) { ConfirmPasswordError = "Les mots de passe sont différents."; first = first.Length > 0 ? first : ConfirmPasswordError; }
        if (!IsCodeSent) { SmsCodeError = "Demande d'abord le code reçu par SMS."; first = first.Length > 0 ? first : SmsCodeError; }
        else if (string.IsNullOrWhiteSpace(SmsCode)) { SmsCodeError = "Saisis le code reçu par SMS."; first = first.Length > 0 ? first : SmsCodeError; }

        if (first.Length > 0) { SemanticScreenReader.Announce($"Erreur : {first}"); return false; }
        return true;
    }

    private bool ValidateStep2()
    {
        GroupError = RhesusError = BirthDateError = CityError = TermsError = GeneralError = string.Empty;
        var first = string.Empty;

        if (SelectedGroup is null) { GroupError = "Choisis ton groupe sanguin."; first = GroupError; }
        if (SelectedRhesus is null) { RhesusError = "Choisis ton rhésus, ou « Je ne sais pas »."; first = first.Length > 0 ? first : RhesusError; }
        if (Validators.AgeOn(BirthDate, DateTime.Today) < AppConstants.MinimumDonorAge)
        { BirthDateError = $"Tu dois avoir au moins {AppConstants.MinimumDonorAge} ans pour donner."; first = first.Length > 0 ? first : BirthDateError; }
        if (string.IsNullOrWhiteSpace(City)) { CityError = "Saisis ta ville ou ta zone."; first = first.Length > 0 ? first : CityError; }
        if (!AcceptTerms) { TermsError = "Accepte les conditions et le traitement de tes données pour continuer."; first = first.Length > 0 ? first : TermsError; }

        if (first.Length > 0) { SemanticScreenReader.Announce($"Erreur : {first}"); return false; }
        return true;
    }

    private static void Fail(Action<string> setError, string message)
    {
        setError(message);
        SemanticScreenReader.Announce($"Erreur : {message}");
    }
}

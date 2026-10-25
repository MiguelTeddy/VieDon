using CommunityToolkit.Mvvm.Input;

namespace VieDon.App.ViewModels;

/// <summary>Ligne de l'écran d'administration de démonstration : un compte et son résumé.</summary>
public sealed class AdminAccountItem
{
    public AdminAccountItem(string title, string line1, string line2, string line3, string verificationText,
        bool canDecide, bool isRequester, Func<AdminAccountItem, bool, Task> decide, string phone)
    {
        Title = title;
        Line1 = line1;
        Line2 = line2;
        Line3 = line3;
        VerificationText = verificationText;
        HasVerification = isRequester;
        CanDecide = canDecide;
        Phone = phone;
        ApproveCommand = new AsyncRelayCommand(() => decide(this, true));
        RejectCommand = new AsyncRelayCommand(() => decide(this, false));
    }

    public string Phone { get; }
    public string Title { get; }
    public string Line1 { get; }
    public string Line2 { get; }
    public string Line3 { get; }
    public string VerificationText { get; }
    public bool HasVerification { get; }
    public bool CanDecide { get; }
    public IAsyncRelayCommand ApproveCommand { get; }
    public IAsyncRelayCommand RejectCommand { get; }
    public string Description => $"{Title}. {Line1}. {Line2}. {Line3}";
}

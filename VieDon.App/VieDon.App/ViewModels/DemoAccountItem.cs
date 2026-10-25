using CommunityToolkit.Mvvm.Input;
using VieDon.App.Services;

namespace VieDon.App.ViewModels;

/// <summary>Ligne de la liste « Comptes de démonstration » de l'écran Connexion (développement seulement).</summary>
public sealed class DemoAccountItem
{
    public DemoAccountItem(DemoAccount account, Action<DemoAccount> use)
    {
        Account = account;
        UseCommand = new RelayCommand(() => use(account));
    }

    public DemoAccount Account { get; }
    public IRelayCommand UseCommand { get; }
    public string Title => Account.Title;
    public string Subtitle => Account.Subtitle;
    public string ButtonDescription => $"Se connecter avec le compte de démonstration : {Account.Title}";
}

using VieDon.App.Localization;

namespace VieDon.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Langue choisie à l'écran Bienvenue (enregistrée sur l'appareil).
        Loc.Load();

        // Thème blanc imposé : l'application ne suit plus le mode sombre du téléphone ou du PC.
        UserAppTheme = AppTheme.Light;

        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
#if WINDOWS
        // Sur PC, la fenêtre a la taille d'un téléphone pour voir l'application comme sur mobile.
        window.Title = "VieDon";
        window.Width = 412;
        window.Height = 860;
        window.X = 120;
        window.Y = 20;
#endif
        return window;
    }
}

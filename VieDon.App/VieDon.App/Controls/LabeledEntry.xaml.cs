namespace VieDon.App.Controls;

public partial class LabeledEntry : ContentView
{
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(LabeledEntry), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(LabeledEntry), string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(LabeledEntry), string.Empty);

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(LabeledEntry), Microsoft.Maui.Keyboard.Default);

    public static readonly BindableProperty ReturnTypeProperty =
        BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(LabeledEntry), ReturnType.Default);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(LabeledEntry), false,
            propertyChanged: (b, _, n) =>
            {
                var c = (LabeledEntry)b;
                c.IsHidden = (bool)n;
                c.ShowToggle = (bool)n;
            });

    public static readonly BindableProperty ErrorTextProperty =
        BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(LabeledEntry), string.Empty,
            propertyChanged: (b, _, n) =>
            {
                var c = (LabeledEntry)b;
                var text = n as string ?? string.Empty;
                c.HasError = !string.IsNullOrEmpty(text);
                c.ErrorDisplay = $"Erreur : {text}"; // jamais la couleur seule : icône/texte « Erreur : » + contour épais
            });

    // Propriétés internes liées au XAML
    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(LabeledEntry), false);
    public static readonly BindableProperty ErrorDisplayProperty =
        BindableProperty.Create(nameof(ErrorDisplay), typeof(string), typeof(LabeledEntry), string.Empty);
    public static readonly BindableProperty IsHiddenProperty =
        BindableProperty.Create(nameof(IsHidden), typeof(bool), typeof(LabeledEntry), false);
    public static readonly BindableProperty ShowToggleProperty =
        BindableProperty.Create(nameof(ShowToggle), typeof(bool), typeof(LabeledEntry), false);
    public static readonly BindableProperty ToggleTextProperty =
        BindableProperty.Create(nameof(ToggleText), typeof(string), typeof(LabeledEntry), "Afficher");

    public LabeledEntry()
    {
        InitializeComponent();
        Root.BindingContext = this; // le contexte de la page reste intact pour les liaisons externes
    }

    public event EventHandler? Completed;

    public string LabelText { get => (string)GetValue(LabelTextProperty); set => SetValue(LabelTextProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public Keyboard Keyboard { get => (Keyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }
    public ReturnType ReturnType { get => (ReturnType)GetValue(ReturnTypeProperty); set => SetValue(ReturnTypeProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public string ErrorText { get => (string)GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }
    public bool HasError { get => (bool)GetValue(HasErrorProperty); private set => SetValue(HasErrorProperty, value); }
    public string ErrorDisplay { get => (string)GetValue(ErrorDisplayProperty); private set => SetValue(ErrorDisplayProperty, value); }
    public bool IsHidden { get => (bool)GetValue(IsHiddenProperty); private set => SetValue(IsHiddenProperty, value); }
    public bool ShowToggle { get => (bool)GetValue(ShowToggleProperty); private set => SetValue(ShowToggleProperty, value); }
    public string ToggleText { get => (string)GetValue(ToggleTextProperty); private set => SetValue(ToggleTextProperty, value); }

    public void FocusInput() => Input.Focus();

    private void OnInputCompleted(object? sender, EventArgs e) => Completed?.Invoke(this, EventArgs.Empty);

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        IsHidden = !IsHidden;
        ToggleText = IsHidden ? "Afficher" : "Masquer";
    }
}

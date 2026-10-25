using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace VieDon.App.Localization;

/// <summary>
/// Texte fixe d'un écran : {loc:Tr 'Texte en français'}. Se met à jour quand la langue change.
/// </summary>
[ContentProperty(nameof(Text))]
public sealed class TrExtension : IMarkupExtension<BindingBase>
{
    public string Text { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider) =>
        new Binding(nameof(LocalizationState.Language), BindingMode.OneWay, new TrLiteralConverter(), Text,
            source: LocalizationState.Instance);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

/// <summary>Valeur = langue (sert à relancer la liaison) ; paramètre = texte français à traduire.</summary>
internal sealed class TrLiteralConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Loc.T(parameter as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Texte venant d'un ViewModel : {Binding X, Converter={x:Static loc:TrConverter.Instance}}.
/// Avec ConverterParameter='Erreur : {0}', le modèle est traduit puis rempli avec la valeur traduite.
/// </summary>
public sealed class TrConverter : IValueConverter
{
    public static TrConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string ?? value?.ToString();
        var translated = Loc.T(text);
        if (parameter is string format)
            return string.Format(CultureInfo.CurrentCulture, Loc.T(format), translated);
        return translated;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

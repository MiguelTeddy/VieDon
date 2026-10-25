using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VieDon.Core.Formatting;

namespace VieDon.App.Localization;

/// <summary>État observable de la langue : les liaisons de l'interface se rafraîchissent quand elle change.</summary>
public sealed class LocalizationState : INotifyPropertyChanged
{
    public static LocalizationState Instance { get; } = new();

    private string _language = "fr";

    public string Language
    {
        get => _language;
        set
        {
            if (_language == value) return;
            _language = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Traduction français → anglais. Le code et les écrans sont écrits en français (langue source) ;
/// tout texte affiché passe par T(). Un texte absent du dictionnaire reste en français (jamais de plantage).
/// Les entrées contenant {0}, {1}… sont des modèles (nombres, noms, dates) compilés en expressions régulières.
/// </summary>
public static class Loc
{
    private const string PreferenceKey = "viedon_language";

    private sealed record Rule(Regex Regex, string English, int Count, int LiteralLength);

    private static readonly Dictionary<string, string> Exact = new(StringComparer.Ordinal);
    private static readonly List<Rule> Rules = new();
    private static readonly Regex Placeholder = new(@"\{(\d)\}", RegexOptions.Compiled);
    private static readonly Regex SentenceBreak = new(@"(?<=[.!?])\s+", RegexOptions.Compiled);
    private static readonly Regex TrailingPunctuation = new(@"[.!?]+$", RegexOptions.Compiled);

    static Loc()
    {
        foreach (var (fr, en) in EnglishDictionary.Entries)
        {
            var key = Normalize(fr);
            if (!Placeholder.IsMatch(key))
            {
                Exact[key] = en;
                continue;
            }

            var count = Placeholder.Matches(key).Count;
            var literal = Placeholder.Replace(key, string.Empty).Length;
            var pattern = new StringBuilder("^");
            var last = 0;
            foreach (Match m in Placeholder.Matches(key))
            {
                pattern.Append(Regex.Escape(key.Substring(last, m.Index - last)));
                pattern.Append("(.+?)");
                last = m.Index + m.Length;
            }
            pattern.Append(Regex.Escape(key.Substring(last)));
            pattern.Append('$');
            Rules.Add(new Rule(new Regex(pattern.ToString(), RegexOptions.Singleline | RegexOptions.Compiled), en, count, literal));
        }

        // Modèles les plus précis d'abord (plus de texte fixe = plus précis).
        Rules.Sort((a, b) => b.LiteralLength.CompareTo(a.LiteralLength));
    }

    public static string Language => LocalizationState.Instance.Language;
    public static bool IsEnglish => Language == "en";

    /// <summary>Lit la langue enregistrée (au démarrage de l'application).</summary>
    public static void Load()
    {
        string code;
        try { code = Preferences.Default.Get(PreferenceKey, "fr"); }
        catch { code = "fr"; }
        Apply(code == "en" ? "en" : "fr");
    }

    public static void SetLanguage(string code)
    {
        code = code == "en" ? "en" : "fr";
        try { Preferences.Default.Set(PreferenceKey, code); } catch { /* sans conséquence */ }
        Apply(code);
    }

    private static void Apply(string code)
    {
        // Dates et distances suivent la langue (24 octobre 2026 / 24 October 2026, 2,4 km / 2.4 km).
        Labels.Culture = CultureInfo.GetCultureInfo(code == "en" ? "en-GB" : "fr-FR");
        LocalizationState.Instance.Language = code;
    }

    public static string T(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        if (!IsEnglish) return text;
        return Translate(text, 0);
    }

    private static string Normalize(string text) =>
        text.Replace('’', '\'').Replace('\u00A0', ' ').Replace('\u202F', ' ').Trim();

    private static string Translate(string text, int depth)
    {
        var key = Normalize(text);
        if (Exact.TryGetValue(key, out var exact)) return exact;
        if (depth >= 4) return text;

        foreach (var rule in Rules)
        {
            var match = rule.Regex.Match(key);
            if (!match.Success) continue;

            var groups = new string[rule.Count];
            for (var i = 0; i < rule.Count; i++)
                groups[i] = Translate(match.Groups[i + 1].Value, depth + 1);

            var result = rule.English;
            for (var i = 0; i < rule.Count; i++)
                result = result.Replace("{" + i + "}", groups[i]);
            return result;
        }

        // Texte composé de plusieurs phrases : on traduit phrase par phrase.
        var parts = SentenceBreak.Split(key);
        if (parts.Length > 1)
        {
            var changed = false;
            var translated = new string[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                // 1) la phrase entière, avec sa ponctuation finale (les entrées du dictionnaire la contiennent parfois)
                var whole = Translate(part, depth + 1);
                if (!string.Equals(whole, part, StringComparison.Ordinal))
                {
                    changed = true;
                    translated[i] = whole;
                    continue;
                }

                // 2) sinon sans la ponctuation finale, qu'on remet ensuite
                var tail = TrailingPunctuation.Match(part).Value;
                var core = tail.Length > 0 ? part.Substring(0, part.Length - tail.Length) : part;
                var tr = Translate(core, depth + 1);
                if (!string.Equals(tr, core, StringComparison.Ordinal)) changed = true;
                translated[i] = tr + tail;
            }
            if (changed) return string.Join(" ", translated);
        }

        return text;
    }
}

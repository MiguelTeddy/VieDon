using VieDon.Core.Models;

namespace VieDon.App.Services;

/// <summary>Images de l'interface : médailles des niveaux et initiales des avatars.</summary>
public static class UiAssets
{
    public static string Medal(DonorLevel level) => level switch
    {
        DonorLevel.Bronze => "medal_bronze.png",
        DonorLevel.Silver => "medal_silver.png",
        DonorLevel.Gold => "medal_gold.png",
        DonorLevel.Platinum => "medal_platinum.png",
        _ => "medal_none.png"
    };

    /// <summary>Initiales (2 lettres au plus) pour l'avatar rond.</summary>
    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var letters = words.Where(w => char.IsLetter(w[0])).Take(2).Select(w => char.ToUpperInvariant(w[0]));
        var result = new string(letters.ToArray());
        return result.Length == 0 ? "?" : result;
    }
}

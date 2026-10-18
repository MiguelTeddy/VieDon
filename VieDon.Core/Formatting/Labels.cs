using System.Globalization;
using VieDon.Core.Models;

namespace VieDon.Core.Formatting;

/// <summary>Textes d'affichage partagés. L'urgence et les statuts se disent toujours en mots (jamais la couleur seule).</summary>
public static class Labels
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static string GroupLabel(BloodGroup group, Rhesus rhesus) =>
        $"{group}{(rhesus == Rhesus.Positive ? "+" : rhesus == Rhesus.Negative ? "-" : string.Empty)}";

    public static string UrgencyLabel(UrgencyLevel level) => level switch
    {
        UrgencyLevel.Critical => "Critique",
        UrgencyLevel.Urgent => "Urgente",
        _ => "Normale"
    };

    public static string DistanceText(double km) =>
        km < 1 ? $"{(int)Math.Round(km * 1000)} m" : $"{km.ToString("0.0", Fr)} km";

    public static string EstablishmentTypeLabel(EstablishmentType type) => type switch
    {
        EstablishmentType.Hospital => "Hôpital",
        EstablishmentType.Clinic => "Clinique",
        EstablishmentType.BloodBank => "Banque de sang ou centre de transfusion",
        EstablishmentType.HealthCenter => "Centre de santé ou dispensaire",
        EstablishmentType.Laboratory => "Laboratoire d'analyses",
        EstablishmentType.Ambulance => "Service d'ambulance ou d'urgence",
        EstablishmentType.Association => "Association, ONG ou Croix-Rouge",
        _ => "Autre (à préciser)"
    };

    public static string PouchesText(int pouches) => pouches > 1 ? $"{pouches} poches" : "1 poche";

    public static string Ago(DateTimeOffset created, DateTimeOffset now)
    {
        var d = now - created;
        if (d < TimeSpan.FromMinutes(1)) return "à l'instant";
        if (d < TimeSpan.FromHours(1)) return $"il y a {(int)d.TotalMinutes} min";
        if (d < TimeSpan.FromDays(1)) return $"il y a {(int)d.TotalHours} h";
        return $"il y a {(int)d.TotalDays} j";
    }
}

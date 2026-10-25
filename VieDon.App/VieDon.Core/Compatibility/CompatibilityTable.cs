using VieDon.Core.Models;

namespace VieDon.Core.Compatibility;

/// <summary>
/// RG2 : par défaut, seuls les donneurs du MÊME groupe (rhésus compris) sont ciblés ; l'option
/// « Inclure les groupes compatibles » ajoute les groupes compatibles. La compatibilité n'est jamais symétrique.
/// ATTENTION : la table ci-dessous porte sur les globules rouges (sang total). Elle doit être VALIDÉE
/// par un professionnel de la transfusion avant tout usage réel (voir le cahier des charges, RG2).
/// </summary>
public static class CompatibilityTable
{
    /// <summary>Un donneur de ce groupe peut-il donner des globules rouges à un patient de ce groupe ?</summary>
    public static bool IsCompatibleRedCells(BloodGroup donorGroup, Rhesus donorRhesus, BloodGroup patientGroup, Rhesus patientRhesus)
    {
        if (donorRhesus == Rhesus.Unknown || patientRhesus == Rhesus.Unknown) return false;

        var abo = donorGroup switch
        {
            BloodGroup.O => true,
            BloodGroup.A => patientGroup is BloodGroup.A or BloodGroup.AB,
            BloodGroup.B => patientGroup is BloodGroup.B or BloodGroup.AB,
            BloodGroup.AB => patientGroup == BloodGroup.AB,
            _ => false
        };

        // Un donneur rhésus négatif peut donner à tous ; un donneur rhésus positif seulement à un patient positif.
        var rh = donorRhesus == Rhesus.Negative || patientRhesus == Rhesus.Positive;
        return abo && rh;
    }

    /// <summary>RG1 / RG2 : ce donneur doit-il recevoir l'alerte ?</summary>
    public static bool IsTargeted(
        BloodGroup? donorGroup, Rhesus? donorRhesus,
        BloodGroup patientGroup, Rhesus patientRhesus,
        bool includeCompatible)
    {
        // RG11 : un donneur qui ne connaît pas son groupe ne reçoit pas d'alerte ciblée par groupe.
        if (donorGroup is null || donorRhesus is null or Rhesus.Unknown) return false;
        if (patientRhesus == Rhesus.Unknown) return false;

        if (!includeCompatible)
            return donorGroup == patientGroup && donorRhesus == patientRhesus;

        return IsCompatibleRedCells(donorGroup.Value, donorRhesus.Value, patientGroup, patientRhesus);
    }

    /// <summary>Aperçu des groupes ciblés avant l'envoi d'une alerte (écran 22).</summary>
    public static IReadOnlyList<(BloodGroup Group, Rhesus Rhesus)> TargetedGroups(
        BloodGroup patientGroup, Rhesus patientRhesus, bool includeCompatible)
    {
        var result = new List<(BloodGroup, Rhesus)>();
        foreach (var g in Enum.GetValues<BloodGroup>())
            foreach (var r in new[] { Rhesus.Positive, Rhesus.Negative })
                if (IsTargeted(g, r, patientGroup, patientRhesus, includeCompatible))
                    result.Add((g, r));
        return result;
    }
}

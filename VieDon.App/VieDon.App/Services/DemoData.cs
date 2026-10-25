namespace VieDon.App.Services;

public sealed record DemoAccount(string Title, string Subtitle, string Phone);

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Comptes d'exemple : personnes, établissements, dons et chiffres FICTIFS.
/// À supprimer quand l'API réelle sera branchée.
/// </summary>
public static class DemoData
{
    public const string Password = "Demo1234";

    // Compte d'administration (développement seulement) : liste tous les comptes de démonstration.
    public const string AdminLogin = "admin@viedon.test";
    public const string AdminPassword = "Admin1234";

    // Donneurs
    public const string Aminata = "699000001";
    public const string Marc = "699000002";
    public const string JeanPaul = "699000003";
    public const string Fatou = "699000004";
    public const string Nadia = "699000005";

    // Demandeurs
    public const string HopitalCentral = "699100001";
    public const string CliniqueCentre = "699100002";
    public const string Paul = "699100003";
    public const string Association = "699100004";

    public static IReadOnlyList<DemoAccount> Accounts { get; } = new[]
    {
        new DemoAccount("Administrateur (développement)", "Voit tous les comptes, leurs dons et alertes ; valide ou refuse les établissements en attente", AdminLogin),
        new DemoAccount("Aminata Nkoulou (donneuse)", "O+ confirmé - niveau Or - 7 dons validés - 3 avantages utilisables", Aminata),
        new DemoAccount("Marc Tchoumi (donneur)", "O- confirmé - niveau Platine - 10 dons validés - tous les avantages", Marc),
        new DemoAccount("Jean-Paul Essomba (donneur)", "A- confirmé - niveau Argent - 3 dons validés", JeanPaul),
        new DemoAccount("Fatou Diallo (donneuse)", "AB+ confirmé - niveau Bronze - 1 don validé", Fatou),
        new DemoAccount("Nadia Bello (nouvelle donneuse)", "O+ déclaré - aucun don : accepte l'alerte puis fais valider le don", Nadia),
        new DemoAccount("Hôpital Central (établissement)", "Vérifié - alerte en cours et historique - peut valider les dons", HopitalCentral),
        new DemoAccount("Clinique du Centre (établissement)", "Vérifié - alerte en cours et historique", CliniqueCentre),
        new DemoAccount("Paul Mbarga (individu)", "Compte individuel - alertes limitées, Critique verrouillé", Paul),
        new DemoAccount("Association Entraide Exemple", "Établissement en attente de vérification", Association)
    };
}

namespace VieDon.Core.Models;

public sealed record Establishment(string Name, string City, bool IsVerified);

/// <summary>Alerte ciblée. RG18 : ni nom ni diagnostic du patient, seulement groupe, poches, lieu et urgence.</summary>
public sealed record Alert(
    Guid Id,
    UrgencyLevel Level,
    BloodGroup Group,
    Rhesus RhesusFactor,
    int Pouches,
    Establishment Establishment,
    double DistanceKm,
    DateTimeOffset CreatedAt,
    AlertStatus Status,
    string Zone,
    bool IsFromIndividual,
    bool IncludeCompatible);

/// <summary>Participation d'un donneur à une alerte (après acceptation).</summary>
public sealed record Participation(
    Guid AlertId,
    string Code,
    string EstablishmentName,
    string Address,
    DateTimeOffset Deadline);

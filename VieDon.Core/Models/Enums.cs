namespace VieDon.Core.Models;

public enum UserSpace { Donor, Requester }
public enum BloodGroup { A, B, AB, O }
public enum Rhesus { Positive, Negative, Unknown }

// RG8 : le groupe est toujours affiché avec son statut.
public enum GroupStatus { Unknown, Declared, Pending, Confirmed }

public enum UrgencyLevel { Normal, Urgent, Critical }

public enum AlertStatus { Active, Filled, Cancelled, Expired }

public enum RequesterKind { Individual, Establishment }
public enum VerificationStatus { Pending, Verified, Rejected }
public enum EstablishmentType { Hospital, Clinic, BloodBank, HealthCenter, Laboratory, Ambulance, Association, Other }

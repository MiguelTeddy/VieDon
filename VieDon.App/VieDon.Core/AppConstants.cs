namespace VieDon.Core;

public static class AppConstants
{
    // RG13 : l'âge minimum respecte la loi du pays (à valider avec un juriste local).
    public const int MinimumDonorAge = 18;
    public const int PasswordMinLength = 8;

    // RG12 : délai indicatif entre deux dons, à valider auprès d'un centre de transfusion (il varie selon le pays et la personne).
    public const int DonationIntervalDays = 56;
}

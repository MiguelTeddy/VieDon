using VieDon.Core.Models;
using VieDon.Core.Policy;

namespace VieDon.App.Services;

/// <summary>DÉVELOPPEMENT UNIQUEMENT. Dons et avantages d'EXEMPLE en mémoire (lieux, partenaires et dates fictifs).</summary>
public sealed class InMemoryDonationService : IDonationService
{
    private readonly Dictionary<string, List<Donation>> _donations = new();
    private readonly Dictionary<string, Dictionary<Guid, string>> _used = new();

    private static readonly Guid R1 = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid R2 = Guid.Parse("00000000-0000-0000-0000-0000000000a2");
    private static readonly Guid R3 = Guid.Parse("00000000-0000-0000-0000-0000000000a3");
    private static readonly Guid R4 = Guid.Parse("00000000-0000-0000-0000-0000000000a4");
    private static readonly Guid R5 = Guid.Parse("00000000-0000-0000-0000-0000000000a5");

    private readonly IReadOnlyList<Reward> _catalog;

    public InMemoryDonationService()
    {
        var today = DateTime.Today;
        _catalog = new[]
        {
            new Reward(R1, "Centre de transfusion Exemple", "Collation offerte après le don",
                "Une collation est offerte après chaque don validé dans ce centre.",
                "Sur présentation du code, le jour du don. Non cumulable. Aucune valeur monétaire.",
                DonorLevel.Bronze, today.AddDays(300)),
            new Reward(R2, "Laboratoire Exemple", "Analyse de glycémie offerte",
                "Une analyse de glycémie à jeun, offerte pour te remercier de tes dons.",
                "Une fois par an, sur rendez-vous, sur présentation du code.",
                DonorLevel.Silver, today.AddDays(200)),
            new Reward(R3, "Pharmacie Exemple", "Réduction sur les produits de bien-être",
                "Une réduction de 10 % (valeur d'exemple) sur une sélection de produits de bien-être.",
                "Une fois par trimestre. Hors médicaments. Sur présentation du code en caisse.",
                DonorLevel.Gold, today.AddDays(120)),
            new Reward(R4, "Clinique Exemple", "Consultation de prévention",
                "Une consultation de prévention et de conseils santé.",
                "Sur rendez-vous, une fois par an, sur présentation du code.",
                DonorLevel.Platinum, today.AddDays(250)),
            new Reward(R5, "Association Entraide Exemple", "Atelier de premiers secours (saison passée)",
                "Un atelier d'initiation aux premiers secours, proposé la saison précédente.",
                "Offre terminée.",
                DonorLevel.Bronze, today.AddDays(-30))
        };

        Seed(DemoData.Aminata, new[] { 80, 160, 250, 340, 430, 520, 610 }, attended: null);
        Seed(DemoData.Marc, new[] { 20, 90, 160, 230, 300, 380, 450, 520, 600, 700 }, attended: null);
        Seed(DemoData.JeanPaul, new[] { 40, 130, 220 }, attended: 300);
        Seed(DemoData.Fatou, new[] { 120 }, attended: null);
        _donations[DemoData.Nadia] = new List<Donation>(); // nouvelle donneuse : aucun don

        _used[DemoData.Aminata] = new Dictionary<Guid, string> { { R1, "AV-4821" } };
        _used[DemoData.Marc] = new Dictionary<Guid, string> { { R1, "AV-7310" }, { R2, "AV-2954" } };
    }

    private static readonly string[] Places =
    {
        "Centre de transfusion Exemple", "Hôpital Central", "Banque de sang Exemple", "Clinique du Centre"
    };

    private void Seed(string phone, int[] daysAgo, int? attended)
    {
        var list = new List<Donation>();
        for (var i = 0; i < daysAgo.Length; i++)
            list.Add(new Donation(Guid.NewGuid(), DateTime.Today.AddDays(-daysAgo[i]), Places[i % Places.Length], "Ville", DonationStatus.Validated));
        if (attended is int a)
            list.Add(new Donation(Guid.NewGuid(), DateTime.Today.AddDays(-a), Places[0], "Ville", DonationStatus.AttendedNoDonation));
        _donations[phone] = list;
    }

    private List<Donation> ListFor(string phone)
    {
        if (!_donations.TryGetValue(phone, out var list))
            _donations[phone] = list = new List<Donation>();
        return list;
    }

    public async Task<IReadOnlyList<Donation>> GetDonationsAsync(string phone, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);
        return ListFor(phone).OrderByDescending(d => d.Date).ToList();
    }

    public Task AddPendingAsync(string phone, Guid alertId, string place, string city, CancellationToken ct = default)
    {
        var list = ListFor(phone);
        if (!list.Any(d => d.AlertId == alertId))
            list.Add(new Donation(Guid.NewGuid(), DateTime.Today, place, city, DonationStatus.Pending, alertId));
        return Task.CompletedTask;
    }

    public Task RemovePendingAsync(string phone, Guid alertId, CancellationToken ct = default)
    {
        ListFor(phone).RemoveAll(d => d.AlertId == alertId && d.Status == DonationStatus.Pending);
        return Task.CompletedTask;
    }

    public Task<Donation?> ValidateAsync(string phone, Guid alertId, CancellationToken ct = default)
    {
        var list = ListFor(phone);
        var index = list.FindIndex(d => d.AlertId == alertId);
        if (index < 0) return Task.FromResult<Donation?>(null);
        var validated = list[index] with { Status = DonationStatus.Validated, Date = DateTime.Today };
        list[index] = validated;
        return Task.FromResult<Donation?>(validated);
    }

    private DonorLevel LevelOf(string phone) =>
        LevelPolicy.FromDonations(ListFor(phone).Count(d => d.Status == DonationStatus.Validated));

    private RewardItem Evaluate(string phone, Reward reward, DonorLevel level)
    {
        if (_used.TryGetValue(phone, out var used) && used.TryGetValue(reward.Id, out var code))
            return new RewardItem(reward, RewardStatus.Used, code);
        if (reward.ValidUntil.Date < DateTime.Today)
            return new RewardItem(reward, RewardStatus.Expired);
        if (level < reward.RequiredLevel)
            return new RewardItem(reward, RewardStatus.Locked);
        return new RewardItem(reward, RewardStatus.Available);
    }

    private static int Order(RewardStatus s) => s switch
    {
        RewardStatus.Available => 0,
        RewardStatus.Used => 1,
        RewardStatus.Locked => 2,
        _ => 3
    };

    public async Task<IReadOnlyList<RewardItem>> GetRewardsAsync(string phone, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);
        var level = LevelOf(phone);
        return _catalog.Select(r => Evaluate(phone, r, level))
            .OrderBy(i => Order(i.Status))
            .ThenBy(i => i.Reward.RequiredLevel)
            .ToList();
    }

    public async Task<RewardItem?> GetRewardAsync(string phone, Guid rewardId, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        var reward = _catalog.FirstOrDefault(r => r.Id == rewardId);
        return reward is null ? null : Evaluate(phone, reward, LevelOf(phone));
    }

    public async Task<RewardItem?> UseRewardAsync(string phone, Guid rewardId, CancellationToken ct = default)
    {
        await Task.Delay(300, ct);
        var current = await GetRewardAsync(phone, rewardId, ct);
        if (current is null || current.Status != RewardStatus.Available) return current;

        if (!_used.TryGetValue(phone, out var used))
            _used[phone] = used = new Dictionary<Guid, string>();
        used[rewardId] = $"AV-{Random.Shared.Next(1000, 9999)}";
        return await GetRewardAsync(phone, rewardId, ct);
    }
}

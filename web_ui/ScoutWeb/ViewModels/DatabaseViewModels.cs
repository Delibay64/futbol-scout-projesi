namespace ScoutWeb.ViewModels
{
    public class PlayerDetailsTRView
    {
        public int OyuncuId { get; set; }
        public string? TamIsim { get; set; }
        public string? Pozisyon { get; set; }
        public int? Yas { get; set; }
        public decimal? PiyasaDegeriEuro { get; set; }
        public decimal? PiyasaDegeriTL { get; set; }
        public string? TakimAdi { get; set; }
        public string? Lig { get; set; }
    }

    public class TopScorerView
    {
        public string? FullName { get; set; }
        public string? TeamName { get; set; }
        public int? TotalGoals { get; set; }
        public int? TotalAssists { get; set; }
        public int? TotalMatches { get; set; }
        public decimal? GoalsPerMatch { get; set; }
    }

    public class YoungTalentView
    {
        public string? FullName { get; set; }
        public int? Age { get; set; }
        public string? Position { get; set; }
        public decimal? MarketValue { get; set; }
        public string? TeamName { get; set; }
    }

    public class TeamSummaryView
    {
        public string? TeamName { get; set; }
        public string? LeagueName { get; set; }
        public int? PlayerCount { get; set; }
        public decimal? TotalValue { get; set; }
        public decimal? AvgValue { get; set; }
        public double? AvgAge { get; set; }
    }
}
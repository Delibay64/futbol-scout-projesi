using System.ComponentModel.DataAnnotations.Schema;

namespace ScoutWeb.Models
{
    // VIEW: vw_PlayerDetailsTR - Euro ve TL cinsinden oyuncu değerleri
    [NotMapped]
    public class PlayerDetailsTRView
    {
        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("age")]
        public int? Age { get; set; }

        [Column("position")]
        public string? Position { get; set; }

        [Column("team_name")]
        public string? TeamName { get; set; }

        [Column("eurovalue")]
        public decimal? EuroValue { get; set; }

        [Column("tlvalue")]
        public decimal? TLValue { get; set; }
    }

    // VIEW: vw_TopScorers - Gol krallığı listesi
    [NotMapped]
    public class TopScorerView
    {
        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("goals")]
        public int? Goals { get; set; }

        [Column("assists")]
        public int? Assists { get; set; }

        [Column("goalspermatch")]
        public decimal? GoalsPerMatch { get; set; }
    }

    // VIEW: vw_YoungTalents - 21 yaş altı oyuncular
    [NotMapped]
    public class YoungTalentView
    {
        [Column("player_id")]
        public int PlayerId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("position")]
        public string? Position { get; set; }

        [Column("age")]
        public int? Age { get; set; }

        [Column("nationality")]
        public string? Nationality { get; set; }

        [Column("team_id")]
        public int? TeamId { get; set; }

        [Column("current_market_value")]
        public decimal? CurrentMarketValue { get; set; }
    }

    // VIEW: vw_TeamSummary - Takım istatistikleri
    [NotMapped]
    public class TeamSummaryView
    {
        [Column("team_name")]
        public string? TeamName { get; set; }

        [Column("playercount")]
        public int? PlayerCount { get; set; }

        [Column("averageage")]
        public decimal? AverageAge { get; set; }
    }

    // VIEW: vw_ScoutSummary - Scout rapor özeti
    [NotMapped]
    public class ScoutSummaryView
    {
        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("report_date")]
        public DateTime? ReportDate { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("scoutname")]
        public string? ScoutName { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace ScoutWeb.Models
{
    // View 1: vw_PlayerDetailsTR için model
    [NotMapped]
    public class PlayerDetailReport
    {
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty; // ✅ Eşittir string.Empty ekledik

        [Column("team_name")]
        public string TeamName { get; set; } = string.Empty; // ✅

        [Column("position")]
        public string Position { get; set; } = string.Empty; // ✅

        [Column("age")]
        public int Age { get; set; }

        [Column("eurovalue")] 
        public decimal EuroValue { get; set; }

        [Column("tlvalue")]
        public decimal TLValue { get; set; } 
    }

    // View 3: vw_TopScorers için model
    [NotMapped]
    public class TopScorerReport
    {
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty; // ✅

        [Column("goals")]
        public int Goals { get; set; }

        [Column("assists")]
        public int Assists { get; set; }

        [Column("goalspermatch")]
        public double GoalsPerMatch { get; set; }
    }
}
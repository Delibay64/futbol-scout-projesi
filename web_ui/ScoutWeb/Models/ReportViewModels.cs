using System.ComponentModel.DataAnnotations.Schema;

namespace ScoutWeb.Models
{
    // AdminDashboard için oyuncu detay modeli
    [NotMapped]
    public class PlayerDetailReport
    {
        [Column("fullname")]
        public string FullName { get; set; } = string.Empty;

        [Column("teamname")]
        public string TeamName { get; set; } = string.Empty;

        [Column("position")]
        public string Position { get; set; } = string.Empty;

        [Column("age")]
        public int Age { get; set; }

        [Column("eurovalue")]
        public decimal EuroValue { get; set; }

        [Column("tlvalue")]
        public decimal TLValue { get; set; }
    }

    // AdminDashboard için gol krallığı modeli
    [NotMapped]
    public class TopScorerReport
    {
        [Column("fullname")]
        public string FullName { get; set; } = string.Empty;

        [Column("goals")]
        public int Goals { get; set; }

        [Column("assists")]
        public int Assists { get; set; }

        [Column("goalspermatch")]
        public decimal GoalsPerMatch { get; set; }
    }
}
namespace ScoutGrpcService.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string? FullName { get; set; }
        public string? Position { get; set; }
        public int? Age { get; set; }
        public decimal? CurrentMarketValue { get; set; }
        public int? TeamId { get; set; }
        public Team? Team { get; set; }
    }

    public class Team
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? LeagueName { get; set; }
    }

    public class Playerstat
    {
        public int StatId { get; set; }
        public int? PlayerId { get; set; }
        public int? Goals { get; set; }
        public int? Assists { get; set; }
        public int? MatchesPlayed { get; set; }
    }
}
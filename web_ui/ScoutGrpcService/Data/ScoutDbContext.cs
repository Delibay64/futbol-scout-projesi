using Microsoft.EntityFrameworkCore;
using ScoutGrpcService.Models;

namespace ScoutGrpcService.Data
{
    public class ScoutDbContext : DbContext
    {
        public ScoutDbContext(DbContextOptions<ScoutDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Playerstat> Playerstats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("players");
                entity.HasKey(e => e.PlayerId);
                entity.Property(e => e.PlayerId).HasColumnName("player_id");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Position).HasColumnName("position");
                entity.Property(e => e.Age).HasColumnName("age");
                entity.Property(e => e.CurrentMarketValue).HasColumnName("current_market_value");
                entity.Property(e => e.TeamId).HasColumnName("team_id");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("teams");
                entity.HasKey(e => e.TeamId);
                entity.Property(e => e.TeamId).HasColumnName("team_id");
                entity.Property(e => e.TeamName).HasColumnName("team_name");
                entity.Property(e => e.LeagueName).HasColumnName("league_name");
            });

            modelBuilder.Entity<Playerstat>(entity =>
            {
                entity.ToTable("playerstats");
                entity.HasKey(e => e.StatId);
                entity.Property(e => e.StatId).HasColumnName("stat_id");
                entity.Property(e => e.PlayerId).HasColumnName("player_id");
                entity.Property(e => e.Goals).HasColumnName("goals");
                entity.Property(e => e.Assists).HasColumnName("assists");
                entity.Property(e => e.MatchesPlayed).HasColumnName("matches_played");
            });
        }
    }
}
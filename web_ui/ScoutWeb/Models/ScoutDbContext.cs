using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ScoutWeb.Models;

public partial class ScoutDbContext : DbContext
{
    public ScoutDbContext()
    {
    }

    public ScoutDbContext(DbContextOptions<ScoutDbContext> options)
        : base(options)
    {
    }

    // --- 1. MEVCUT TABLOLARIN (Dokunmadık) ---
    public virtual DbSet<Player> Players { get; set; }
    public virtual DbSet<PlayerPriceLog> PriceLogs { get; set; }
    public virtual DbSet<Playerstat> Playerstats { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Scoutreport> Scoutreports { get; set; }
    public virtual DbSet<Team> Teams { get; set; }
    public virtual DbSet<User> Users { get; set; }

    // --- 2. VERİTABANI VIEW'LARI (PostgreSQL'deki VIEW'lar) ---
    public virtual DbSet<PlayerDetailsTRView> VwPlayerDetailsTR { get; set; }
    public virtual DbSet<TopScorerView> VwTopScorers { get; set; }
    public virtual DbSet<YoungTalentView> VwYoungTalents { get; set; }
    public virtual DbSet<TeamSummaryView> VwTeamSummary { get; set; }
    public virtual DbSet<ScoutSummaryView> VwScoutSummary { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string Program.cs'den Dependency Injection ile gelecek
        // Buraya hardcoded connection string yazmayın!
    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- ESKİ AYARLARIN (AYNEN KORUNDU) ---
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("players_pkey");
            entity.ToTable("players");

            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.CurrentMarketValue).HasPrecision(15, 2).HasColumnName("current_market_value");
            entity.Property(e => e.FullName).HasMaxLength(100).HasColumnName("full_name");
            entity.Property(e => e.Nationality).HasMaxLength(50).HasColumnName("nationality");
            entity.Property(e => e.Position).HasMaxLength(50).HasColumnName("position");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Team).WithMany(p => p.Players)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("players_team_id_fkey");
        });

        modelBuilder.Entity<Playerstat>(entity =>
        {
            entity.HasKey(e => e.StatId).HasName("playerstats_pkey");
            entity.ToTable("playerstats");

            entity.Property(e => e.StatId).HasColumnName("stat_id");
            entity.Property(e => e.Assists).HasColumnName("assists");
            entity.Property(e => e.Goals).HasColumnName("goals");
            entity.Property(e => e.MatchesPlayed).HasColumnName("matches_played");
            entity.Property(e => e.MinutesPlayed).HasColumnName("minutes_played");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.RedCards).HasColumnName("red_cards");
            entity.Property(e => e.Season).HasMaxLength(20).HasColumnName("season");
            entity.Property(e => e.YellowCards).HasColumnName("yellow_cards");

            entity.HasOne(d => d.Player).WithMany(p => p.Playerstats)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("playerstats_player_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");
            entity.ToTable("roles");
            entity.HasIndex(e => e.RoleName, "roles_role_name_key").IsUnique();
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName).HasMaxLength(50).HasColumnName("role_name");
        });

        modelBuilder.Entity<Scoutreport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("scoutreports_pkey");
            entity.ToTable("scoutreports");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.PredictedValue).HasPrecision(15, 2).HasColumnName("predicted_value");
            entity.Property(e => e.ReportDate).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("report_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Player).WithMany(p => p.Scoutreports)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("scoutreports_player_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Scoutreports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("scoutreports_user_id_fkey");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("teams_pkey");
            entity.ToTable("teams");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Country).HasMaxLength(50).HasColumnName("country");
            entity.Property(e => e.LeagueName).HasMaxLength(100).HasColumnName("league_name");
            entity.Property(e => e.TeamName).HasMaxLength(100).HasColumnName("team_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");
            entity.ToTable("users");
            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username).HasMaxLength(50).HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_role_id_fkey");
        });

        // --- 3. POSTGRESQL VERİTABANI VIEW'LARI ---

        // VIEW: vw_PlayerDetailsTR (TL cinsinden oyuncu değerleri)
        modelBuilder.Entity<PlayerDetailsTRView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_playerdetailstr");
        });

        // VIEW: vw_TopScorers (Gol krallığı)
        modelBuilder.Entity<TopScorerView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_topscorers");
        });

        // VIEW: vw_YoungTalents (Genç yetenekler)
        modelBuilder.Entity<YoungTalentView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_youngtalents");
        });

        // VIEW: vw_TeamSummary (Takım özeti)
        modelBuilder.Entity<TeamSummaryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_teamsummary");
        });

        // VIEW: vw_ScoutSummary (Scout rapor özeti)
        modelBuilder.Entity<ScoutSummaryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_scoutsummary");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
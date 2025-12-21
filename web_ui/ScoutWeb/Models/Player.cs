using System;
using System.Collections.Generic;

namespace ScoutWeb.Models; // Namespace'e dikkat!

public partial class Player
{
    public int PlayerId { get; set; }

    public string? FullName { get; set; }

    public string? Position { get; set; }

    public double? Age { get; set; }

    public decimal? CurrentMarketValue { get; set; } // DbContext'te Decimal tanımlanmış

    public string? Nationality { get; set; }

    public int? TeamId { get; set; }

    public virtual Team? Team { get; set; }
    
    // --- KRİTİK KISIM BURASI ---
    // Adı "PlayerStats" değil, "Playerstats" olmalı (Db bağlamıyla aynı)
    public virtual ICollection<Playerstat> Playerstats { get; set; } = new List<Playerstat>();

    public virtual ICollection<Scoutreport> Scoutreports { get; set; } = new List<Scoutreport>();
}
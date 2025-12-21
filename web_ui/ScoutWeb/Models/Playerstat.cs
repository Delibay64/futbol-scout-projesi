using System;
using System.Collections.Generic;

namespace ScoutWeb.Models;

public partial class Playerstat
{
    // EKSİK OLAN BUYDU:
    public int StatId { get; set; }

    public int? PlayerId { get; set; }

    public int? Goals { get; set; }

    public int? Assists { get; set; }

    public int? MinutesPlayed { get; set; }

    public int? MatchesPlayed { get; set; }

    public int? YellowCards { get; set; }

    public int? RedCards { get; set; }

    public string? Season { get; set; }

    public virtual Player? Player { get; set; }
}
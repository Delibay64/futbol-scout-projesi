using System;
using System.Collections.Generic;

namespace ScoutWeb.Models;

public partial class Scoutreport
{
    public int ReportId { get; set; }

    public int? UserId { get; set; }

    public int? PlayerId { get; set; }

    public decimal? PredictedValue { get; set; }

    public string? Notes { get; set; }

    public DateTime? ReportDate { get; set; }

    public bool IsApproved { get; set; } = false; // Onay durumu

    public virtual Player? Player { get; set; }

    public virtual User? User { get; set; }
}

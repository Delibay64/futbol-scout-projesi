using System;
using System.Collections.Generic;

namespace ScoutWeb.Models;

public partial class Team
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = null!;

    public string? LeagueName { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}

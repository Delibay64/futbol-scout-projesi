using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScoutWeb.Models;

[Table("roles")] // SQL'deki tablo adı
public partial class Role
{
    [Key]
    [Column("role_id")] // SQL'deki sütun adı
    public int RoleId { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; } = null!;

    // İlişkiler
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
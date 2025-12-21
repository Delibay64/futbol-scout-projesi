using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScoutWeb.Models;

[Table("users")] // SQL'deki tablo adı
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!; // Şifreler hashlenmiş tutulacak

    [Column("email")]
    public string? Email { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    // --- İlişkiler ---
    
    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }

    public virtual ICollection<Scoutreport> Scoutreports { get; set; } = new List<Scoutreport>();
}
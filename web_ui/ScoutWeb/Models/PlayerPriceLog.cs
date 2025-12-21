using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScoutWeb.Models
{
    [Table("player_price_logs")] // SQL'deki tablo adımız
    public class PlayerPriceLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [Column("player_id")]
        public int PlayerId { get; set; }

        // İlişki: Hangi oyuncunun fiyatı değişti?
        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }

        [Column("old_value")]
        public decimal OldValue { get; set; }

        [Column("new_value")]
        public decimal NewValue { get; set; }

        [Column("change_date")]
        public DateTime ChangeDate { get; set; }
    }
}
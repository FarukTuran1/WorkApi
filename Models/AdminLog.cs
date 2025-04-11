using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkApi.Models
{
    [Table("admin_logs")]
    public class AdminLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("admin_id")] // NULL olabilir
        public int? AdminId { get; set; } // int? nullable olduğunu belirtir (Foreign Key)

        [Required]
        [Column("action")] // TEXT
        public string Action { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("AdminId")]
        public virtual User? Admin { get; set; } // Hangi admin kullanıcısına ait (nullable olabilir)
    }
}
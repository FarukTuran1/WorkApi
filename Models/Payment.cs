using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkApi.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; } // Foreign Key

        [Required]
        [MaxLength(50)]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Column("payment_status", TypeName = "varchar(50)")] // ENUM eşleşmesi
        public string PaymentStatus { get; set; } = "pending"; // SQL DEFAULT ile eşleşsin

        [MaxLength(255)]
        [Column("transaction_id")]
        public string? TransactionId { get; set; } // NULL olabilir, UNIQUE (DbContext'te ayarlanabilir)

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; } // Hangi siparişe ait
    }
}
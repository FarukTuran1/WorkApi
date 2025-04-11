using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkApi.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; } // Foreign Key sütunu

        [Required]
        [Column("total_price", TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [Column("status", TypeName = "varchar(50)")] // ENUM eşleşmesi
        public string Status { get; set; } = "pending"; // SQL'deki DEFAULT ile eşleşsin

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---

        // Hangi kullanıcıya ait olduğunu gösterir (Foreign Key ilişkisi)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; } // Bir siparişin bir kullanıcısı olur

        // Bir siparişin birden fazla kalemi olabilir
        public virtual ICollection<OrderItem>? OrderItems { get; set; }

        // Bir siparişin birden fazla ödemesi olabilir (genelde 1 olur ama yapıya uygun)
        public virtual ICollection<Payment>? Payments { get; set; }
    }
}
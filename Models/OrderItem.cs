using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkApi.Models
{
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; } // Foreign Key

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; } // Foreign Key

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(10, 2)")] // Sipariş anındaki ürün fiyatı
        public decimal Price { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; } // Hangi siparişe ait

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } // Hangi ürüne ait
    }
}
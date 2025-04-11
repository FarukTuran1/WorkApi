using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkApi.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")] // TEXT olduğu için MaxLength yok, NULL olabilir
        public string? Description { get; set; } // string? nullable olduğunu belirtir

        [Required]
        [Column("price", TypeName = "decimal(10, 2)")] // Hassasiyeti belirtmek önemlidir
        public decimal Price { get; set; }

        [Required]
        [Column("stock_quantity")]
        public int StockQuantity { get; set; }

        [Required]
        [Column("category", TypeName = "varchar(100)")] // ENUM eşleşmesi
        public string Category { get; set; } = string.Empty; // Örn: "Elektronik", "Giyim"...

        [MaxLength(512)] // URL için makul bir uzunluk
        [Column("image_url")]
        public string? ImageUrl { get; set; } // NULL olabilir

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---

        // Bir ürün birden fazla sipariş kaleminde bulunabilir
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}
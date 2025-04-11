using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkApi.Models // Proje adınıza göre namespace'i kontrol edin
{
    [Table("users")] // Veritabanındaki tablo adı
    public class User
    {
        [Key] // Primary Key
        [Column("id")]
        public int Id { get; set; }

        [Required] // NOT NULL
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty; // null olmaması için

        [Required]
        [MaxLength(255)]
        [EmailAddress] // Ekstra validasyon
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        // ENUM'ları string olarak map etmek en kolayıdır.
        // Alternatif olarak bir C# enum tanımlayıp onu kullanabilirsiniz.
        [Column("role", TypeName = "varchar(50)")] // ENUM eşleşmesi için tip belirtmek iyi olabilir
        public string Role { get; set; } = string.Empty; // Örn: "admin", "customer"

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties (İlişkiler) ---

        // Bir kullanıcının birden fazla siparişi olabilir
        public virtual ICollection<Order>? Orders { get; set; }

        // Bir admin kullanıcısının (eğer rolü admin ise) birden fazla logu olabilir
        public virtual ICollection<AdminLog>? AdminLogs { get; set; }
    }
}
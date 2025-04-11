using Microsoft.EntityFrameworkCore;
using WorkApi.Models; // Model sınıflarınızın bulunduğu namespace (Proje adınıza göre kontrol edin!)

namespace WorkApi.Data // Proje adınıza göre namespace'i kontrol edin
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor: DbContextOptions'ı alıp base class'a gönderir.
        // Bu, dışarıdan (Program.cs'den) veritabanı bağlantı ayarlarını almamızı sağlar.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- DbSet Properties ---
        // Her bir model sınıfınız için bir DbSet<T> özelliği ekleyin.
        // Bu özellikler, veritabanı tablolarınıza karşılık gelir ve
        // LINQ sorguları yapmanızı sağlar.
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }


        // --- Fluent API Configuration (İsteğe Bağlı ama Önemli) ---
        // OnModelCreating metodu, modeller oluşturulurken ek yapılandırmalar
        // yapmanızı sağlar. Data Annotations ([Key], [Required] vb.) ile
        // yapılamayan veya daha detaylı kontrol gerektiren ayarlar burada yapılır.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Mevcut yapılandırmaları koru

            // Örnek Fluent API Yapılandırmaları:

            // 1. Decimal Sütunlar için Hassasiyet (Precision) Ayarı:
            //    Model sınıflarında [Column(TypeName="decimal(10,2)")] kullandık,
            //    ama burada da yapabilirdik. Bu şekilde yapmak daha merkezi olabilir.
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(10, 2);

            // 2. Unique Index'ler (Eğer Data Annotation ile yapılmadıysa):
            //    User.Email için zaten [Table("users")] üzerinde [Index(IsUnique=true)] veya
            //    modelde [EmailAddress] (dolaylı) ya da SQL'de UNIQUE tanımlıydı.
            //    Payment.TransactionId için SQL'de UNIQUE tanımladık ama burada da belirtilebilir:
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();
            // Not: Eğer TransactionId null olabiliyorsa (ki öyle), unique index null değerleri
            // farklı kabul eder (genellikle veritabanına göre değişir, MySQL'de birden fazla null olabilir).

            // 3. İlişki Davranışları (Cascade Delete vb.):
            //    SQL kodumuzda bazı ON DELETE davranışlarını belirttik.
            //    EF Core varsayılanları bazen farklı olabilir veya burada daha net tanımlayabiliriz.
            //    Örneğin, Order silindiğinde OrderItems'ın silinmesi (SQL'de CASCADE idi):
            modelBuilder.Entity<Order>()
               .HasMany(o => o.OrderItems)        // Order'ın birden çok OrderItem'ı var
               .WithOne(oi => oi.Order)           // OrderItem'ın bir Order'ı var
               .HasForeignKey(oi => oi.OrderId)   // OrderItem'daki FK OrderId'dir
               .OnDelete(DeleteBehavior.Cascade); // Order silinirse OrderItems'ı da sil.

            // User silindiğinde Order'ların silinmemesi (SQL'de RESTRICT idi):
            modelBuilder.Entity<User>()
               .HasMany(u => u.Orders)
               .WithOne(o => o.User)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict); // Kullanıcıyı silmeye çalışırken siparişi varsa engelle.

            // AdminLog için: Admin (User) silinirse AdminId'nin NULL olması (SQL'de SET NULL idi):
            modelBuilder.Entity<AdminLog>()
               .HasOne(al => al.Admin) // AdminLog'un bir Admin'i (User) var (nullable)
               .WithMany(u => u.AdminLogs) // User'ın çok AdminLog'u var
               .HasForeignKey(al => al.AdminId) // AdminLog'daki FK AdminId'dir
               .OnDelete(DeleteBehavior.SetNull); // İlişkili User silinirse AdminId'yi NULL yap.

            // Payment için: Order silinirse Payment'ın silinmemesi (SQL'de RESTRICT idi)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);


            // Buraya başka özel model yapılandırmaları ekleyebilirsiniz.
            // Örneğin, tablo/sütun adlarını burada merkezi olarak tanımlamak,
            // veri tiplerini ayarlamak, varsayılan değerler vermek, index'ler oluşturmak vb.
        }
    }
}
// Models/Dtos/OrderItemDto.cs
namespace WorkApi.Models.Dtos // Namespace'i kontrol edin
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; } // Sadece ID'yi tutuyoruz
        public int ProductId { get; set; } // Sadece ID'yi tutuyoruz
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Sipariş anındaki fiyat

        // İsteğe bağlı: Ürünle ilgili basit bilgiler eklenebilir
        // public string? ProductName { get; set; }
        // public decimal? ProductPrice { get; set; } // Belki ürünün güncel fiyatı?
    }
}
// Models/Dtos/OrderDto.cs
namespace WorkApi.Models.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // İlişkili Kullanıcı Bilgisi (Basit) - İsteğe Bağlı
        public UserSimpleDto? User { get; set; } // User için de bir DTO lazım

        // Sipariş Kalemleri - DTO listesi olarak!
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

        // Ödemeler - DTO listesi olarak!
        public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
    }
}
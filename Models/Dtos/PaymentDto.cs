// Models/Dtos/PaymentDto.cs
namespace WorkApi.Models.Dtos
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        // Order nesnesi yok!
    }
}
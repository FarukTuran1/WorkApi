// Models/Dtos/AdminLogDto.cs
namespace WorkApi.Models.Dtos
{
    public class AdminLogDto
    {
        public int Id { get; set; }
        public int? AdminId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? AdminName { get; set; } // İsteğe bağlı: Admin adını da ekleyebiliriz
        // Admin (User) nesnesi yok!
    }
}
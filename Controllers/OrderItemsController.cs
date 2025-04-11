using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkApi.Data;
using WorkApi.Models;
using WorkApi.Models.Dtos; // DTO namespace'ini ekleyin
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WorkApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orderitems - DTO Listesi Döndürür
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetOrderItems()
        {
            // Doğrudan DTO'ya Select yapıyoruz, gereksiz veri çekmiyoruz.
            return await _context.OrderItems
                                 .Select(oi => new OrderItemDto
                                 {
                                     Id = oi.Id,
                                     OrderId = oi.OrderId,
                                     ProductId = oi.ProductId,
                                     Quantity = oi.Quantity,
                                     Price = oi.Price
                                     // İstersen ürün adını da ekleyebilirsin:
                                     // ProductName = oi.Product.Name // Bunun için Include gerekebilir veya Product tablosuna join yapılır
                                 })
                                 .ToListAsync();
            // Ok() sarmalayıcısı ActionResult<T> tarafından otomatik yönetilir
        }

        // GET: api/orderitems/5 - Tek DTO Döndürür
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemDto>> GetOrderItem(int id) // Dönüş tipi OrderItemDto
        {
            var orderItemDto = await _context.OrderItems
                                          .Where(oi => oi.Id == id) // Önce filtrele
                                          .Select(oi => new OrderItemDto // DTO'ya map et
                                          {
                                              Id = oi.Id,
                                              OrderId = oi.OrderId,
                                              ProductId = oi.ProductId,
                                              Quantity = oi.Quantity,
                                              Price = oi.Price
                                              // İstersen ürün adını da ekleyebilirsin:
                                              // ProductName = oi.Product.Name
                                          })
                                          .FirstOrDefaultAsync(); // DTO'yu veya null'ı al

            if (orderItemDto == null)
            {
                return NotFound();
            }

            return Ok(orderItemDto); // DTO döndür
        }

        // GET: api/orders/{orderId}/items - DTO Listesi Döndürür
        [HttpGet("/api/orders/{orderId}/items")]
        public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetItemsForOrder(int orderId) // Dönüş tipi IEnumerable<OrderItemDto>
        {
            if (!await _context.Orders.AnyAsync(o => o.Id == orderId))
            {
                return NotFound(new { message = "Belirtilen sipariş bulunamadı." });
            }

            var orderItems = await _context.OrderItems
                                           .Where(oi => oi.OrderId == orderId) // Sadece bu siparişe ait olanlar
                                           .Select(oi => new OrderItemDto // DTO'ya map et
                                           {
                                               Id = oi.Id,
                                               OrderId = oi.OrderId,
                                               ProductId = oi.ProductId,
                                               Quantity = oi.Quantity,
                                               Price = oi.Price
                                               // İstersen ürün adını da ekleyebilirsin:
                                               // ProductName = oi.Product.Name
                                           })
                                           .ToListAsync();

            return Ok(orderItems); // DTO listesi döndür
        }

        // POST: api/orderitems - DTO Döndürür
        [HttpPost]
        // Girdiyi şimdilik OrderItem olarak alabiliriz, ama dönüş DTO olacak
        public async Task<ActionResult<OrderItemDto>> PostOrderItem(OrderItem orderItem) // Dönüş tipi OrderItemDto
        {
            // Girdi DTO'su kullanmak daha iyi olabilir (örn: CreateOrderItemDto),
            // böylece istemci gereksiz Id, Price gibi alanları göndermez.
            // Şimdilik OrderItem kabul ediyoruz.

            if (!ModelState.IsValid)
            {
                // Girdi OrderItem olduğu için, IsValid kontrolü direkt çalışır.
                // Eğer girdi DTO olsaydı, DTO üzerinde [Required] vb. tanımlamalar olmalıydı.
                return BadRequest(ModelState);
            }

            // --- Mevcut Kontroller ve İşlemler Aynen Kalır ---
            var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderItem.OrderId);
            var product = await _context.Products.FindAsync(orderItem.ProductId);

            if (!orderExists) return BadRequest(new { message = "Geçersiz sipariş ID." });
            if (product == null) return BadRequest(new { message = "Geçersiz ürün ID." });
            if (product.StockQuantity < orderItem.Quantity) return BadRequest(new { message = $"Yetersiz stok. '{product.Name}' için stokta {product.StockQuantity} adet bulunmaktadır." });

            orderItem.Price = product.Price; // Fiyatı üründen al
            _context.OrderItems.Add(orderItem);

            product.StockQuantity -= orderItem.Quantity; // Stok güncelle
            _context.Entry(product).State = EntityState.Modified;

            var order = await _context.Orders.FindAsync(orderItem.OrderId); // Sipariş toplamını güncelle
            if (order != null)
            {
                order.TotalPrice += (orderItem.Quantity * orderItem.Price);
                _context.Entry(order).State = EntityState.Modified;
            }
            // --- Bitti: Mevcut Kontroller ve İşlemler ---

            await _context.SaveChangesAsync(); // Veritabanına kaydet

            // --- Kaydedilen Entity'yi DTO'ya Dönüştür ---
            var orderItemDto = new OrderItemDto
            {
                Id = orderItem.Id, // Yeni ID'yi al
                OrderId = orderItem.OrderId,
                ProductId = orderItem.ProductId,
                Quantity = orderItem.Quantity,
                Price = orderItem.Price // Ayarlanan fiyat
            };
            // --- Bitti: DTO Dönüşümü ---

            // CreatedAtAction ile DTO'yu döndür
            return CreatedAtAction(nameof(GetOrderItem), new { id = orderItem.Id }, orderItemDto);
        }

        // PUT: api/orderitems/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderItem(int id, OrderItem orderItem) // Girdi hala OrderItem
        {
            // Bu metodun dönüş tipi zaten IActionResult (NoContent) olduğu için
            // dönüş tipinde değişiklik yapmaya gerek yok.
            // İçerisindeki stok ve fiyat güncelleme mantığı da aynı kalabilir.
            // Daha iyi bir yaklaşım girdi olarak da bir UpdateOrderItemDto almak olabilir.

            if (id != orderItem.Id) return BadRequest("ID uyuşmazlığı.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingItem = await _context.OrderItems.AsNoTracking().FirstOrDefaultAsync(oi => oi.Id == id);
            if (existingItem == null) return NotFound();
            if (existingItem.OrderId != orderItem.OrderId || existingItem.ProductId != orderItem.ProductId) return BadRequest("Sipariş kalemi başka bir siparişe veya ürüne taşınamaz.");

            orderItem.Price = existingItem.Price; // Fiyatı koru
            _context.Entry(orderItem).State = EntityState.Modified;
            _context.Entry(orderItem).Property(x => x.Price).IsModified = false;

            int quantityDifference = orderItem.Quantity - existingItem.Quantity;
            if (quantityDifference != 0)
            {
                var product = await _context.Products.FindAsync(orderItem.ProductId);
                var order = await _context.Orders.FindAsync(orderItem.OrderId);
                if (product == null || order == null) return BadRequest("İlişkili ürün veya sipariş bulunamadı.");
                if (product.StockQuantity < quantityDifference) return BadRequest(new { message = $"Yetersiz stok. '{product.Name}' için stokta {product.StockQuantity} adet bulunmaktadır." });

                product.StockQuantity -= quantityDifference;
                order.TotalPrice += (quantityDifference * orderItem.Price);
                _context.Entry(product).State = EntityState.Modified;
                _context.Entry(order).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemExists(id)) return NotFound(); else throw;
            }

            return NoContent();
        }

        // DELETE: api/orderitems/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            // Bu metodun dönüş tipi zaten IActionResult (NoContent) olduğu için
            // değişiklik yapmaya gerek yok.
            // İçerisindeki stok ve fiyat güncelleme mantığı da aynı kalabilir.

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return NotFound();

            var product = await _context.Products.FindAsync(orderItem.ProductId);
            var order = await _context.Orders.FindAsync(orderItem.OrderId);
            if (product != null && order != null)
            {
                product.StockQuantity += orderItem.Quantity;
                order.TotalPrice -= (orderItem.Quantity * orderItem.Price);
                _context.Entry(product).State = EntityState.Modified;
                _context.Entry(order).State = EntityState.Modified;
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}
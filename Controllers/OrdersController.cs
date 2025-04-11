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
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orders - DTO Listesi Döndürür
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders() // Dönüş tipi IEnumerable<OrderDto>
        {
            // Doğrudan OrderDto'ya Select yapıyoruz.
            return await _context.Orders
                                 // Include'lar Select içinde ilişkilere erişiliyorsa gerekli olabilir,
                                 // ama EF Core çoğu basit ilişkiyi Select içinde çözümleyebilir.
                                 // Performans için gereksiz Include'lardan kaçının.
                                 // .Include(o => o.User)
                                 // .Include(o => o.OrderItems)
                                 // .Include(o => o.Payments)
                                 .Select(o => new OrderDto // Order'ı OrderDto'ya map et
                                 {
                                     Id = o.Id,
                                     UserId = o.UserId,
                                     TotalPrice = o.TotalPrice,
                                     Status = o.Status,
                                     CreatedAt = o.CreatedAt,
                                     // İlişkili User bilgisini UserSimpleDto'ya map et
                                     User = o.User == null ? null : new UserSimpleDto
                                     {
                                         Id = o.User.Id,
                                         Name = o.User.Name,
                                         Email = o.User.Email
                                     },
                                     // İlişkili OrderItems listesini OrderItemDto listesine map et
                                     OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                                     {
                                         Id = oi.Id,
                                         OrderId = oi.OrderId,
                                         ProductId = oi.ProductId,
                                         Quantity = oi.Quantity,
                                         Price = oi.Price
                                         // ProductName = oi.Product.Name // İstersen (Product'a erişim için Include gerekebilir)
                                     }).ToList(),
                                     // İlişkili Payments listesini PaymentDto listesine map et
                                     Payments = o.Payments.Select(p => new PaymentDto
                                     {
                                         Id = p.Id,
                                         OrderId = p.OrderId,
                                         PaymentMethod = p.PaymentMethod,
                                         PaymentStatus = p.PaymentStatus,
                                         TransactionId = p.TransactionId,
                                         CreatedAt = p.CreatedAt
                                     }).ToList()
                                 })
                                 .ToListAsync(); // DTO listesini al
        }

        // GET: api/orders/5 - Tek DTO Döndürür
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id) // Dönüş tipi OrderDto
        {
            var orderDto = await _context.Orders
                                      // Include'lar yine Select içinde erişiliyorsa gerekli olabilir.
                                      // .Include(o => o.User)
                                      // .Include(o => o.OrderItems).ThenInclude(oi => oi.Product) // ProductDto eklenecekse
                                      // .Include(o => o.Payments)
                                      .Where(o => o.Id == id) // Önce filtrele
                                      .Select(o => new OrderDto // DTO'ya map et
                                      {
                                          Id = o.Id,
                                          UserId = o.UserId,
                                          TotalPrice = o.TotalPrice,
                                          Status = o.Status,
                                          CreatedAt = o.CreatedAt,
                                          User = o.User == null ? null : new UserSimpleDto
                                          {
                                              Id = o.User.Id,
                                              Name = o.User.Name,
                                              Email = o.User.Email
                                          },
                                          OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                                          {
                                              Id = oi.Id,
                                              OrderId = oi.OrderId,
                                              ProductId = oi.ProductId,
                                              Quantity = oi.Quantity,
                                              Price = oi.Price
                                              // ProductName = oi.Product.Name // İstersen
                                          }).ToList(),
                                          Payments = o.Payments.Select(p => new PaymentDto
                                          {
                                              Id = p.Id,
                                              OrderId = p.OrderId,
                                              PaymentMethod = p.PaymentMethod,
                                              PaymentStatus = p.PaymentStatus,
                                              TransactionId = p.TransactionId,
                                              CreatedAt = p.CreatedAt
                                          }).ToList()
                                      })
                                      .FirstOrDefaultAsync(); // DTO veya null

            if (orderDto == null)
            {
                return NotFound();
            }

            return Ok(orderDto); // DTO döndür
        }

        // POST: api/orders - DTO Döndürür
        [HttpPost]
        // Girdi olarak da CreateOrderDto kullanmak daha iyidir ama şimdilik Order alalım
        public async Task<ActionResult<OrderDto>> PostOrder(Order order) // Dönüş tipi OrderDto
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Mevcut Kontroller ---
            if (!await _context.Users.AnyAsync(u => u.Id == order.UserId))
            {
                return BadRequest(new { message = "Geçersiz kullanıcı ID." });
            }
            if (order.TotalPrice < 0) order.TotalPrice = 0;
            // --- Bitti: Mevcut Kontroller ---

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Kaydet

            // --- Kaydedilen Entity'yi DTO'ya Dönüştür ---
            // İlişkili veriler (User, OrderItems, Payments) henüz yüklenmemiş olabilir.
            // CreatedAtAction'da tam dolu DTO döndürmek için tekrar sorgulamak en iyisi.
            // Veya basit bir DTO döndürebiliriz. Şimdilik basit olanı yapalım:
            var orderDto = new OrderDto
            {
                Id = order.Id, // Yeni ID
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                User = null, // Tekrar sorgulamadan bilemeyiz
                OrderItems = new List<OrderItemDto>(), // Başlangıçta boş
                Payments = new List<PaymentDto>() // Başlangıçta boş
            };
            // Alternatif (Tam DTO için - Ekstra DB sorgusu):
            // var createdOrderDto = await GetOrder(order.Id); // Yukarıdaki GetOrder metodunu çağır
            // if (createdOrderDto.Result is OkObjectResult okResult)
            // {
            //     return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, okResult.Value);
            // }
            // else // GetOrder hata döndürürse (beklenmez ama...)
            // {
            //      return StatusCode(500, "Sipariş oluşturuldu ancak detayları getirilemedi.");
            // }
            // --- Bitti: DTO Dönüşümü ---


            // Şimdilik basit DTO ile CreatedAtAction
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }

        // PUT: api/orders/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order) // Girdi hala Order
        {
            // Dönüş tipi IActionResult olduğu için değişiklik yok.
            // Girdi olarak UpdateOrderDto (örn: sadece Status içeren) kullanmak daha iyi olabilir.
            // İç mantık aynı kalabilir.

            if (id != order.Id) return BadRequest("ID uyuşmazlığı.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(order).State = EntityState.Modified;
            _context.Entry(order).Property(x => x.UserId).IsModified = false;
            _context.Entry(order).Property(x => x.TotalPrice).IsModified = false;
            _context.Entry(order).Property(x => x.CreatedAt).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { if (!OrderExists(id)) return NotFound(); else throw; }
            catch (DbUpdateException ex) { return BadRequest(new { message = "Sipariş güncellenemedi.", error = ex.InnerException?.Message ?? ex.Message }); }

            return NoContent();
        }

        // DELETE: api/orders/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            // Dönüş tipi IActionResult olduğu için değişiklik yok.
            // İç mantık aynı kalabilir.

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            if (await _context.Payments.AnyAsync(p => p.OrderId == id)) return BadRequest(new { message = "Bu siparişe ait ödeme kaydı bulunduğundan silinemez." });

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
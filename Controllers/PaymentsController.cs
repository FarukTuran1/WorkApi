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
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/payments - DTO Listesi Döndürür
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments() // Dönüş tipi IEnumerable<PaymentDto>
        {
            // Doğrudan DTO'ya Select
            return await _context.Payments
                                 .Select(p => new PaymentDto
                                 {
                                     Id = p.Id,
                                     OrderId = p.OrderId,
                                     PaymentMethod = p.PaymentMethod,
                                     PaymentStatus = p.PaymentStatus,
                                     TransactionId = p.TransactionId,
                                     CreatedAt = p.CreatedAt
                                 })
                                 .ToListAsync();
            // Include(p => p.Order) gereksiz çünkü Order nesnesini DTO'da kullanmıyoruz.
        }

        // GET: api/payments/5 - Tek DTO Döndürür
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id) // Dönüş tipi PaymentDto
        {
            var paymentDto = await _context.Payments
                                        .Where(p => p.Id == id) // Filtrele
                                        .Select(p => new PaymentDto // DTO'ya map et
                                        {
                                            Id = p.Id,
                                            OrderId = p.OrderId,
                                            PaymentMethod = p.PaymentMethod,
                                            PaymentStatus = p.PaymentStatus,
                                            TransactionId = p.TransactionId,
                                            CreatedAt = p.CreatedAt
                                        })
                                        .FirstOrDefaultAsync(); // DTO veya null

            if (paymentDto == null)
            {
                return NotFound();
            }

            return Ok(paymentDto); // DTO döndür
        }

        // GET: api/orders/{orderId}/payments - DTO Listesi Döndürür
        [HttpGet("/api/orders/{orderId}/payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsForOrder(int orderId) // Dönüş tipi IEnumerable<PaymentDto>
        {
            if (!await _context.Orders.AnyAsync(o => o.Id == orderId))
            {
                return NotFound(new { message = "Belirtilen sipariş bulunamadı." });
            }

            var payments = await _context.Payments
                                           .Where(p => p.OrderId == orderId) // Filtrele
                                           .Select(p => new PaymentDto // DTO'ya map et
                                           {
                                               Id = p.Id,
                                               OrderId = p.OrderId,
                                               PaymentMethod = p.PaymentMethod,
                                               PaymentStatus = p.PaymentStatus,
                                               TransactionId = p.TransactionId,
                                               CreatedAt = p.CreatedAt
                                           })
                                           .ToListAsync(); // DTO listesi

            return Ok(payments); // DTO listesi döndür
        }


        // POST: api/payments - DTO Döndürür
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPayment(Payment payment) // Girdi Payment, Dönüş PaymentDto
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Mevcut Kontroller ---
            if (!await _context.Orders.AnyAsync(o => o.Id == payment.OrderId)) return BadRequest(new { message = "Geçersiz sipariş ID." });
            if (!string.IsNullOrEmpty(payment.TransactionId) && await _context.Payments.AnyAsync(p => p.TransactionId == payment.TransactionId)) return BadRequest(new { message = "Bu Transaction ID zaten mevcut." });
            // --- Bitti: Mevcut Kontroller ---

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(); // Kaydet

            // --- DTO Dönüşümü ---
            var paymentDto = new PaymentDto
            {
                Id = payment.Id, // Yeni ID
                OrderId = payment.OrderId,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt
            };
            // --- Bitti: DTO Dönüşümü ---

            // DTO ile CreatedAtAction
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, paymentDto);
        }

        // PUT: api/payments/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(int id, Payment payment) // Girdi hala Payment
        {
            // Dönüş tipi IActionResult, değişiklik yok.
            // Girdi olarak UpdatePaymentDto kullanılabilir.

            if (id != payment.Id) return BadRequest("ID uyuşmazlığı.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(payment).State = EntityState.Modified;
            _context.Entry(payment).Property(x => x.OrderId).IsModified = false;
            _context.Entry(payment).Property(x => x.CreatedAt).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { if (!PaymentExists(id)) return NotFound(); else throw; }
            catch (DbUpdateException ex) { return BadRequest(new { message = "Ödeme güncellenemedi.", error = ex.InnerException?.Message ?? ex.Message }); }

            return NoContent();
        }

        // DELETE: api/payments/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            // Dönüş tipi IActionResult, değişiklik yok.

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
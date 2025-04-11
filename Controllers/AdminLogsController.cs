using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkApi.Data;
using WorkApi.Models;
using WorkApi.Models.Dtos; // DTO namespace'ini ekleyin
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http; // StatusCodes için (MethodNotAllowed içinde kullanıldı)

namespace WorkApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // DİKKAT: Bu controller'a erişim sadece admin yetkisine sahip kullanıcılarla sınırlandırılmalıdır!
    // [Authorize(Roles = "admin")] // Örnek - Authentication/Authorization kurulduktan sonra
    public class AdminLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/adminlogs - DTO Listesi Döndürür
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminLogDto>>> GetAdminLogs() // Dönüş tipi IEnumerable<AdminLogDto>
        {
            return await _context.AdminLogs
                                 // .Include(al => al.Admin) // Select içinde Admin'e erişilecekse gerekli
                                 .OrderByDescending(al => al.CreatedAt) // En yeniden eskiye sırala
                                 .Select(al => new AdminLogDto // DTO'ya map et
                                 {
                                     Id = al.Id,
                                     AdminId = al.AdminId,
                                     Action = al.Action,
                                     CreatedAt = al.CreatedAt,
                                     // İlişkili Admin'in adını ekle (Admin null olabilir)
                                     AdminName = al.Admin != null ? al.Admin.Name : null
                                 })
                                 .ToListAsync();
        }

        // GET: api/adminlogs/5 - Tek DTO Döndürür
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminLogDto>> GetAdminLog(int id) // Dönüş tipi AdminLogDto
        {
            var adminLogDto = await _context.AdminLogs
                                         // .Include(al => al.Admin) // Select içinde Admin'e erişilecekse gerekli
                                         .Where(al => al.Id == id) // Önce filtrele
                                         .Select(al => new AdminLogDto // DTO'ya map et
                                         {
                                             Id = al.Id,
                                             AdminId = al.AdminId,
                                             Action = al.Action,
                                             CreatedAt = al.CreatedAt,
                                             AdminName = al.Admin != null ? al.Admin.Name : null
                                         })
                                         .FirstOrDefaultAsync(); // DTO veya null

            if (adminLogDto == null)
            {
                return NotFound();
            }

            return Ok(adminLogDto); // DTO döndür
        }

        // GET: api/users/{adminId}/logs - DTO Listesi Döndürür
        [HttpGet("/api/users/{adminId}/logs")]
        public async Task<ActionResult<IEnumerable<AdminLogDto>>> GetLogsForAdmin(int adminId) // Dönüş tipi IEnumerable<AdminLogDto>
        {
            // Admin kullanıcısı var mı kontrolü (rolü admin mi diye de bakılabilir)
            if (!await _context.Users.AnyAsync(u => u.Id == adminId /* && u.Role == "admin"*/ ))
            {
                return NotFound(new { message = "Belirtilen admin kullanıcısı bulunamadı." });
            }

            var logs = await _context.AdminLogs
                                     .Where(al => al.AdminId == adminId) // Filtrele
                                                                         // .Include(al => al.Admin) // Select içinde Admin'e erişilecekse gerekli
                                     .OrderByDescending(al => al.CreatedAt) // Sırala
                                     .Select(al => new AdminLogDto // DTO'ya map et
                                     {
                                         Id = al.Id,
                                         AdminId = al.AdminId,
                                         Action = al.Action,
                                         CreatedAt = al.CreatedAt,
                                         AdminName = al.Admin != null ? al.Admin.Name : null
                                     })
                                     .ToListAsync(); // DTO listesini al
            return Ok(logs); // DTO listesi döndür
        }


        // POST: api/adminlogs - DTO Döndürür
        [HttpPost]
        public async Task<ActionResult<AdminLogDto>> PostAdminLog(AdminLog adminLog) // Girdi AdminLog, Dönüş AdminLogDto
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // AdminId geçerli bir kullanıcı mı kontrolü (eğer null değilse)
            if (adminLog.AdminId.HasValue && !await _context.Users.AnyAsync(u => u.Id == adminLog.AdminId.Value))
            {
                return BadRequest(new { message = "Geçersiz admin ID." });
            }

            _context.AdminLogs.Add(adminLog);
            await _context.SaveChangesAsync(); // Kaydet

            // --- DTO Dönüşümü ---
            // AdminName'i almak için belki tekrar sorgulamak veya Include kullanmak gerekebilir,
            // veya log ekleme anında admin adını zaten biliyorsak direkt atayabiliriz.
            // Şimdilik null bırakalım veya sadece ID ile yetinelim.
            var adminLogDto = new AdminLogDto
            {
                Id = adminLog.Id,
                AdminId = adminLog.AdminId,
                Action = adminLog.Action,
                CreatedAt = adminLog.CreatedAt,
                AdminName = null // Veya log ekleme logic'inde admin adını alıp buraya ekle
            };
            // --- Bitti: DTO Dönüşümü ---

            // DTO ile CreatedAtAction
            return CreatedAtAction(nameof(GetAdminLog), new { id = adminLog.Id }, adminLogDto);
        }

        // PUT: api/adminlogs/5 (Değişiklik yok - Method Not Allowed)
        [HttpPut("{id}")]
        public IActionResult PutAdminLog(int id, AdminLog adminLog)
        {
            return MethodNotAllowed();
        }

        // DELETE: api/adminlogs/5 (Değişiklik yok - NoContent veya Method Not Allowed)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdminLog(int id)
        {
            var adminLog = await _context.AdminLogs.FindAsync(id);
            if (adminLog == null)
            {
                return NotFound();
            }

            _context.AdminLogs.Remove(adminLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminLogExists(int id)
        {
            return _context.AdminLogs.Any(e => e.Id == id);
        }

        private IActionResult MethodNotAllowed()
        {
            // StatusCodes sınıfını kullanmak için using Microsoft.AspNetCore.Http; ekleyin
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }
    }
}
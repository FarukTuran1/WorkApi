using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkApi.Data;
using WorkApi.Models;
using WorkApi.Models.Dtos; // DTO namespace'ini ekleyin
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Select için

namespace WorkApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Şifre hashleme için bir servis enjekte etmek daha iyi bir pratiktir,
        // ancak şimdilik burada bırakabiliriz.
        // private readonly IPasswordHasher _passwordHasher;
        // public UsersController(ApplicationDbContext context, IPasswordHasher passwordHasher) { ... }

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users - DTO Listesi Döndürür (PasswordHash Yok!)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers() // Dönüş tipi IEnumerable<UserDto>
        {
            return await _context.Users
                                 .Select(u => new UserDto // User'ı UserDto'ya map et
                                 {
                                     Id = u.Id,
                                     Name = u.Name,
                                     Email = u.Email,
                                     Role = u.Role
                                     // PasswordHash burada YOK!
                                 })
                                 .ToListAsync(); // DTO listesi al
        }

        // GET: api/users/5 - Tek DTO Döndürür (PasswordHash Yok!)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id) // Dönüş tipi UserDto
        {
            // DTO'ya Select
            var userDto = await _context.Users
                                        .Where(u => u.Id == id) // Filtrele
                                        .Select(u => new UserDto // DTO'ya map et
                                        {
                                            Id = u.Id,
                                            Name = u.Name,
                                            Email = u.Email,
                                            Role = u.Role
                                            // PasswordHash burada YOK!
                                        })
                                        .FirstOrDefaultAsync(); // DTO veya null al

            if (userDto == null)
            {
                return NotFound();
            }

            return Ok(userDto); // DTO döndür
        }

        // POST: api/users - DTO Döndürür (PasswordHash Yok!)
        [HttpPost]
        // Girdi olarak CreateUserDto (Name, Email, Password, Role içeren) kullanmak en iyisidir.
        // Şimdilik User alıyoruz ama PasswordHash'i düz şifre gibi kabul edip hashleyeceğiz.
        public async Task<ActionResult<UserDto>> PostUser(User user) // Dönüş tipi UserDto
        {
            // Girdi DTO kullansaydık, DTO üzerinde validasyon olurdu.
            // Şimdilik User aldığımız için Name, Email vb. [Required] kontrolleri çalışır.
            if (string.IsNullOrEmpty(user.PasswordHash)) // Şifrenin boş gelmediğini kontrol et
            {
                ModelState.AddModelError("PasswordHash", "Şifre boş olamaz.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Email unique mi kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest(new { message = "Bu email adresi zaten kullanılıyor." });
            }

            // ---- Şifre Hash'leme ----
            // Gelen user.PasswordHash'in düz metin şifre olduğunu varsayıyoruz.
            // Güvenli bir şekilde hash'leyip tekrar aynı alana atıyoruz.
            // BCrypt.Net-Next NuGet paketini yükleyin: dotnet add package BCrypt.Net-Next
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                // Hashleme sırasında hata olursa
                return StatusCode(500, new { message = "Şifre hashlenirken bir hata oluştu.", error = ex.Message });
            }
            // ---- Bitti: Şifre Hash'leme ----


            _context.Users.Add(user); // Hashlenmiş şifre ile User nesnesini ekle
            await _context.SaveChangesAsync(); // Kaydet

            // --- DTO Dönüşümü (PasswordHash OLMADAN) ---
            var userDto = new UserDto
            {
                Id = user.Id, // Yeni ID
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
            // --- Bitti: DTO Dönüşümü ---

            // DTO ile CreatedAtAction
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }

        // PUT: api/users/5 - IActionResult Döndürür
        [HttpPut("{id}")]
        // Girdi olarak UpdateUserDto (Name, Email, Role içeren - Şifre hariç) kullanmak daha iyi.
        public async Task<IActionResult> PutUser(int id, User user) // Girdi hala User
        {
            // Dönüş tipi IActionResult, değişiklik yok.

            if (id != user.Id) return BadRequest("ID uyuşmazlığı.");

            // Gelen modelde şifre hash'ini dikkate almamalıyız (ayrı bir endpoint olmalı).
            // Email ve Role için validasyon yapalım.
            if (string.IsNullOrWhiteSpace(user.Name)) ModelState.AddModelError("Name", "İsim boş olamaz.");
            if (string.IsNullOrWhiteSpace(user.Email)) ModelState.AddModelError("Email", "Email boş olamaz.");
            if (string.IsNullOrWhiteSpace(user.Role)) ModelState.AddModelError("Role", "Rol boş olamaz."); // Veya ENUM kontrolü

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id); // Takip edilen entity'yi al
            if (existingUser == null) return NotFound();

            // Email değiştiriliyorsa unique kontrolü
            if (existingUser.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
            {
                return BadRequest(new { message = "Bu email adresi zaten başka bir kullanıcı tarafından kullanılıyor." });
            }

            // Sadece istenen alanları güncelle (PasswordHash'i GÜNCELLEME!)
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            // existingUser.PasswordHash = ...; // BURADA ŞİFRE GÜNCELLENMEZ!

            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { if (!UserExists(id)) return NotFound(); else throw; }
            catch (DbUpdateException ex) { return BadRequest(new { message = "Kullanıcı güncellenemedi.", error = ex.InnerException?.Message ?? ex.Message }); }

            return NoContent();
        }

        // DELETE: api/users/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Dönüş tipi IActionResult, değişiklik yok.

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // İlişkili sipariş kontrolü (RESTRICT nedeniyle DB zaten engelleyebilir)
            if (await _context.Orders.AnyAsync(o => o.UserId == id))
            {
                return BadRequest(new { message = "Kullanıcı silinemedi. Kullanıcıya ait siparişler bulunmaktadır." });
            }
            // Not: AdminLog'lar SET NULL olacağı için silmeyi engellemez.

            _context.Users.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) { return BadRequest(new { message = "Kullanıcı silinemedi. Veritabanı hatası.", error = ex.InnerException?.Message ?? ex.Message }); }

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
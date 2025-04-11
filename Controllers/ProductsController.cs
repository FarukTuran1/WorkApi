using Microsoft.AspNetCore.Http;
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
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products - DTO Listesi Döndürür
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts() // Dönüş tipi IEnumerable<ProductDto>
        {
            // Doğrudan DTO'ya Select
            return await _context.Products
                                 .Select(p => new ProductDto // Product'ı ProductDto'ya map et
                                 {
                                     Id = p.Id,
                                     Name = p.Name,
                                     Description = p.Description,
                                     Price = p.Price,
                                     StockQuantity = p.StockQuantity,
                                     Category = p.Category,
                                     ImageUrl = p.ImageUrl
                                     // OrderItems listesi DTO'da yok
                                 })
                                 .ToListAsync(); // DTO listesi al
        }

        // GET: api/products/5 - Tek DTO Döndürür
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id) // Dönüş tipi ProductDto
        {
            // DTO'ya Select
            var productDto = await _context.Products
                                        .Where(p => p.Id == id) // Filtrele
                                        .Select(p => new ProductDto // DTO'ya map et
                                        {
                                            Id = p.Id,
                                            Name = p.Name,
                                            Description = p.Description,
                                            Price = p.Price,
                                            StockQuantity = p.StockQuantity,
                                            Category = p.Category,
                                            ImageUrl = p.ImageUrl
                                        })
                                        .FirstOrDefaultAsync(); // DTO veya null al

            if (productDto == null)
            {
                return NotFound();
            }

            return Ok(productDto); // DTO döndür
        }

        // POST: api/products - DTO Döndürür
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(Product product) // Girdi Product, Dönüş ProductDto
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // Kaydet

            // --- DTO Dönüşümü ---
            var productDto = new ProductDto
            {
                Id = product.Id, // Yeni ID
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                ImageUrl = product.ImageUrl
            };
            // --- Bitti: DTO Dönüşümü ---

            // DTO ile CreatedAtAction
            // nameof(GetProduct) hala kullanılabilir çünkü route adı aynı kalıyor
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        // PUT: api/products/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product) // Girdi hala Product
        {
            // Dönüş tipi IActionResult, değişiklik yok.
            // Girdi olarak UpdateProductDto kullanılabilir.

            if (id != product.Id) return BadRequest("URL ID ile ürün ID'si uyuşmuyor.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // ÖNEMLİ: İlişkili verilerin (OrderItems) yanlışlıkla güncellenmediğinden emin olun.
            // En güvenlisi, ya bir UpdateProductDto kullanmak ya da veritabanından
            // mevcut ürünü çekip sadece istenen alanları güncellemektir.
            // Şimdilik Entry(product).State kullanıyoruz ama bu, product nesnesindeki
            // TÜM alanları (ilişkili listeler hariç) günceller.

            _context.Entry(product).State = EntityState.Modified;
            // CreatedAt alanının güncellenmediğinden emin olalım
            _context.Entry(product).Property(x => x.CreatedAt).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { if (!ProductExists(id)) return NotFound(); else throw; }
            catch (DbUpdateException ex) { return BadRequest(new { message = "Ürün güncellenemedi.", error = ex.InnerException?.Message ?? ex.Message }); }


            return NoContent();
        }

        // DELETE: api/products/5 - IActionResult Döndürür (Değişiklik Yok)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Dönüş tipi IActionResult, değişiklik yok.
            // Dikkat: Eğer bu ürün bir OrderItem'da kullanılıyorsa ve
            // OrderItem tablosunda ProductId için ON DELETE RESTRICT varsa,
            // bu silme işlemi başarısız olur.

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // İlişkili OrderItems var mı kontrolü (RESTRICT nedeniyle DB zaten engelleyebilir)
            if (await _context.OrderItems.AnyAsync(oi => oi.ProductId == id))
            {
                return BadRequest(new { message = "Bu ürün siparişlerde kullanıldığı için silinemez." });
            }

            _context.Products.Remove(product);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) // FK kısıtlaması gibi hatalar için
            {
                return BadRequest(new { message = "Ürün silinemedi. Veritabanı kısıtlaması olabilir.", error = ex.InnerException?.Message ?? ex.Message });
            }


            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
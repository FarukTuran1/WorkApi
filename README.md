# WorkApi

Bu proje, ürünler, kullanıcılar, siparişler, sipariş kalemleri, ödemeler ve admin loglarını yönetmek için bir RESTful API sunan bir .NET 6 (veya üstü) Web API uygulamasıdır. E-ticaret veya sipariş yönetim sistemi altyapısı olarak kullanılabilir.

## Kullanılan Teknolojiler

*   **.NET 8+:** Uygulamanın çalıştığı ana platform ve framework.
*   **ASP.NET Core Web API:** RESTful API oluşturmak için kullanılan framework.
*   **Entity Framework Core (EF Core):** Veritabanı işlemleri için kullanılan Object-Relational Mapper (ORM).
*   **MySQL:** Veritabanı yönetim sistemi.
*   **Pomelo.EntityFrameworkCore.MySql:** MySQL veritabanı için EF Core provider'ı.
*   **Swagger (OpenAPI):** API endpoint'lerini belgelemek, görselleştirmek ve test etmek için kullanılır.
*   **DTO (Data Transfer Objects):** API katmanı ile iş mantığı/veri katmanı arasında veri taşımak ve API kontratını belirlemek için kullanılır.

## Proje Yapısı

Proje, standart .NET Web API proje yapısını takip eder:

*   `/Controllers`: Gelen HTTP isteklerini işleyen ve yanıtları döndüren API kontrolcü sınıflarını içerir.
*   `/Models`: Veritabanı tablolarına karşılık gelen EF Core entity (varlık) sınıflarını ve bu varlıklarla ilişkili DTO (Data Transfer Object) sınıflarını içerir (`/Models/Dtos`).
*   `/Data`: Veritabanı bağlantısını ve EF Core yapılandırmalarını içeren `ApplicationDbContext` sınıfını barındırır.
*   `Program.cs`: Uygulamanın giriş noktasıdır. Servislerin (dependency injection), veritabanı bağlantısının, middleware hattının (request pipeline) ve diğer temel yapılandırmaların yapıldığı yerdir.
*   `appsettings.json` / `appsettings.Development.json`: Uygulama yapılandırma ayarlarını (veritabanı bağlantı dizesi vb.) içerir.

## Kurulum ve Çalıştırma

### Ön Gereksinimler

*   .NET SDK (Projenin hedef framework sürümüyle uyumlu - örn: .NET 6 SDK)
*   MySQL Veritabanı Sunucusu (yerel veya uzak)
*   Bir MySQL yönetim aracı (örn: phpMyAdmin, MySQL Workbench - isteğe bağlı ama önerilir)

### Veritabanı Kurulumu

1.  MySQL sunucunuzda bu uygulama için yeni bir veritabanı oluşturun (veya script'in başındaki `CREATE DATABASE IF NOT EXISTS work_db` satırını kullanın).
2.  Proje içindeki `appsettings.json` (veya geliştirme ortamı için `appsettings.Development.json`) dosyasını açın.
3.  `ConnectionStrings` bölümündeki `DefaultConnection` değerini, kullanacağınız MySQL veritabanının bağlantı bilgileriyle (sunucu adresi, veritabanı adı, kullanıcı adı, şifre) güncelleyin. Veritabanı adı olarak script'teki gibi `work_db` kullanmanız önerilir.
4.  **Veritabanı Şemasını Oluşturma:**
    *   Bu proje, veritabanı şemasını otomatik olarak oluşturmak veya güncellemek için Entity Framework Core Migrations kullanmamaktadır.
    *   Gerekli tabloları ve ilişkileri oluşturmak için, proje kök dizininde bulunan `database_setup.sql` dosyasını bir MySQL yönetim aracı (örn: phpMyAdmin, MySQL Workbench) kullanarak veritabanınızda çalıştırın.
    *   Bu script, `work_db` adında bir veritabanı oluşturmaya çalışır (varsa atlar) ve ardından gerekli tüm tabloları, indeksleri ve ilişkileri tanımlar.
    *   **Önemli:** Uygulamanın hatasız çalışması için, `Models` klasöründeki entity sınıfları ve `Data/ApplicationDbContext.cs` içindeki Fluent API yapılandırmaları ile bu SQL script'inin tanımladığı şemanın **tutarlı** olması kritik öneme sahiptir.

### Uygulamayı Çalıştırma

Uygulamayı çalıştırmak için aşağıdaki yöntemlerden birini kullanabilirsiniz:

1.  **Komut Satırı (CLI):**
    *   Projenin kök dizinine gidin.
    *   `dotnet run` komutunu çalıştırın.
2.  **Visual Studio:**
    *   Projeyi Visual Studio ile açın.
    *   Projeyi başlatmak için F5 tuşuna basın (hata ayıklama ile) veya Ctrl+F5 tuşlarına basın (hata ayıklama olmadan).

Uygulama başarıyla başlatıldığında, genellikle `http://localhost:5220` veya `https://localhost:7220` gibi bir adres üzerinden erişilebilir olacaktır (konsol çıktısını kontrol edin).

### API Dokümantasyonu (Swagger)

Uygulama çalışırken, API endpoint'lerinin dokümantasyonuna ve test arayüzüne genellikle `/swagger` yolu üzerinden erişebilirsiniz (örneğin: `http://localhost:5220/swagger`). Swagger UI, tüm API endpoint'lerini listeler, beklenen istek formatlarını gösterir ve doğrudan tarayıcı üzerinden API istekleri göndermenizi sağlar.

## API Endpointleri

Aşağıda ana API kontrolcüleri ve sundukları temel işlevler listelenmiştir. Detaylı bilgi, parametreler ve istek/yanıt gövdeleri için Swagger UI'ı (`/swagger`) inceleyiniz.

### `/api/adminlogs` (Admin Logları)

*   `GET /`: Tüm admin log kayıtlarını listeler.
*   `GET /{id}`: Belirtilen ID'ye sahip admin log kaydını getirir.
*   `POST /`: Yeni bir admin log kaydı oluşturur.
*   `DELETE /{id}`: Belirtilen ID'ye sahip admin log kaydını siler.
*   `PUT /{id}`: **Desteklenmiyor.** Log kayıtlarının değiştirilmesi engellenmiştir (405 Method Not Allowed).
*   `GET /api/users/{adminId}/logs`: Belirtilen admin kullanıcısına ait log kayıtlarını listeler.

### `/api/orderitems` (Sipariş Kalemleri)

*   `GET /`: Tüm sipariş kalemlerini listeler.
*   `GET /{id}`: Belirtilen ID'ye sahip sipariş kalemini getirir.
*   `POST /`: Yeni bir sipariş kalemi oluşturur. Bu işlem, ilgili ürünün stoğunu azaltır ve ait olduğu siparişin toplam tutarını günceller.
*   `PUT /{id}`: Belirtilen sipariş kaleminin miktarını günceller. Stok ve sipariş toplam tutarı buna göre ayarlanır. Sipariş veya ürün ID'si değiştirilemez.
*   `DELETE /{id}`: Belirtilen sipariş kalemini siler. Bu işlem, ilgili ürünün stoğunu artırır ve ait olduğu siparişin toplam tutarını düşürür.
*   `GET /api/orders/{orderId}/items`: Belirtilen siparişe ait tüm kalemleri listeler.

### `/api/orders` (Siparişler)

*   `GET /`: Tüm siparişleri (ilişkili kullanıcı, kalem ve ödeme bilgileriyle birlikte) listeler.
*   `GET /{id}`: Belirtilen ID'ye sahip siparişi (ilişkili detaylarla birlikte) getirir.
*   `POST /`: Yeni bir sipariş oluşturur (Genellikle başlangıç durumu ve sıfır toplam tutar ile).
*   `PUT /{id}`: Belirtilen siparişin bilgilerini günceller. Genellikle sadece `status` (sipariş durumu) alanının güncellenmesi amaçlanmıştır; `UserId`, `TotalPrice`, `CreatedAt` gibi alanlar değiştirilmez.
*   `DELETE /{id}`: Belirtilen siparişi siler. Eğer siparişe ait bir ödeme kaydı varsa silme işlemi engellenir. Sipariş silindiğinde, ona bağlı tüm `OrderItems` kayıtları da otomatik olarak silinir (Cascade Delete).

### `/api/payments` (Ödemeler)

*   `GET /`: Tüm ödeme kayıtlarını listeler.
*   `GET /{id}`: Belirtilen ID'ye sahip ödeme kaydını getirir.
*   `POST /`: Yeni bir ödeme kaydı oluşturur (Genellikle bir `OrderId` ile ilişkilendirilir).
*   `PUT /{id}`: Belirtilen ödeme kaydının bilgilerini günceller (Örn: ödeme durumu).
*   `DELETE /{id}`: Belirtilen ödeme kaydını siler.
*   `GET /api/orders/{orderId}/payments`: Belirtilen siparişe ait ödeme kayıtlarını listeler.

### `/api/products` (Ürünler)

*   `GET /`: Tüm ürünleri listeler.
*   `GET /{id}`: Belirtilen ID'ye sahip ürünü getirir.
*   `POST /`: Yeni bir ürün oluşturur.
*   `PUT /{id}`: Belirtilen ürünün bilgilerini günceller (`CreatedAt` alanı hariç).
*   `DELETE /{id}`: Belirtilen ürünü siler. Eğer ürün herhangi bir sipariş kaleminde (`OrderItem`) kullanılıyorsa silme işlemi engellenir.

### `/api/users` (Kullanıcılar)

*   `GET /`: Tüm kullanıcıları listeler (Hassas veriler içermeyen `UserDto` kullanılır).
*   `GET /{id}`: Belirtilen ID'ye sahip kullanıcıyı getirir (Hassas veriler içermeyen `UserDto` kullanılır).
*   `POST /`: Yeni bir kullanıcı oluşturur. İstekte gönderilen şifre (`PasswordHash` alanı üzerinden) **BCrypt kullanılarak güvenli bir şekilde hashlenir** ve veritabanına kaydedilir. E-posta adresinin benzersizliği kontrol edilir.
*   `PUT /{id}`: Belirtilen kullanıcının `Name`, `Email` ve `Role` bilgilerini günceller. **Şifre bu endpoint ile güncellenmez.** E-posta değiştiriliyorsa benzersizlik kontrolü yapılır.
*   `DELETE /{id}`: Belirtilen kullanıcıyı siler. Eğer kullanıcının siparişleri (`Order`) varsa silme işlemi engellenir (`Restrict`).

## Kimlik Doğrulama ve Yetkilendirme

*   Uygulama `Program.cs` içinde `app.UseAuthentication()` ve `app.UseAuthorization()` middleware'lerini kullanacak şekilde yapılandırılmıştır (veya yapılandırılabilir).
*   Ancak, mevcut kontrolcülerin incelenmesi sonucunda çoğu endpoint'in herkese açık olduğu görülmektedir (`[Authorize]` attribute'u kullanılmamış olabilir).
*   Gerçek dünya senaryolarında, API'nin güvenliğini sağlamak için JWT (JSON Web Token) tabanlı kimlik doğrulama gibi mekanizmaların tam olarak entegre edilmesi ve kontrolcülerde/endpoint'lerde gerekli yetkilendirme kurallarının (`[Authorize]`, rol bazlı yetkilendirme vb.) uygulanması gerekir.

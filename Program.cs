using Microsoft.EntityFrameworkCore; // DbContext ve UseMySql için
using WorkApi.Data;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var serverVersion = ServerVersion.AutoDetect(connectionString);
// 3. DbContext'i AddDbContext ile servislere kaydet
builder.Services.AddDbContext<ApplicationDbContext>(dbContextOptions => dbContextOptions
    .UseMySql(connectionString, serverVersion, mySqlOptions => // <--- UseMySql'e üçüncü parametre olarak lambda ekle
    {
        // EnableRetryOnFailure'ý burada, mySqlOptions üzerinde çaðýr
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);

        // Buraya baþka MySQL'e özgü ayarlar da eklenebilir (gerekiyorsa)
        // mySqlOptions.CommandTimeout(60); // Örnek
    })
// Opsiyonel: Genel loglama ayarlarý burada kalabilir
// .LogTo(Console.WriteLine, LogLevel.Information)
// .EnableSensitiveDataLogging()
// .EnableDetailedErrors()
);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

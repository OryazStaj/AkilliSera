using Microsoft.EntityFrameworkCore;
using AkilliSera_API.Data;
using AkilliSera_API.Services;
using AkilliSera_API.Hubs; // SignalR anlık veri iletimi için Hub namespace'i

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CORS YAPILANDIRMASI
// ==========================================
// Frontend uygulamasının adresleri (Geliştirme ve Üretim portları)
var allowedOrigins = new[] { "https://localhost:7214", "http://localhost:5108" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // GELİŞTİRME ORTAMI: Localhost testleri için esnek bağlantı
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // SignalR canlı bağlantı kimlik doğrulama desteği
        }
        else
        {
            // ÜRETİM ORTAMI: Yalnızca izin verilen Frontend adresleri
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// ==========================================
// 2. VERİTABANI VE EKİP SERVİSLERİ
// ==========================================
// SQL Server bağlantısı ve ekip veritabanı servisinin DI konteynerine kaydı
builder.Services.AddDbContext<AkilliSeraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DataBaseService>();

// ==========================================
// 3. CANLI YAYIN VE TELEMETRİ SERVİSLERİ
// ==========================================
// Anlık sensör akışı için SignalR servisi
builder.Services.AddSignalR();

// ESP32 cihazının canlılık (online/offline) takibini yapan servis
builder.Services.AddSingleton<HealthCheckService>();

// ==========================================
// 4. API VE SWAGGER YAPILANDIRMASI
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 5. HTTP İSTEK PİPELİNE / MİDDLEWARE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

// ==========================================
// 6. UÇ NOKTALAR (ENDPOINTS)
// ==========================================
app.MapControllers();

// Frontend ve ESP32 için canlı yayın SignalR köprüsü
app.MapHub<SeraHub>("/serahub");

app.Run();
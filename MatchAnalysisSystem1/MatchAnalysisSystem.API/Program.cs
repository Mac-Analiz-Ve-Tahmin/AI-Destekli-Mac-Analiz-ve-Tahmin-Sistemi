using MatchAnalysisSystem.Business;
using MatchAnalysisSystem.DataAccess;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// --- BAĞIMLILIK ENJEKSİYONU (Dependency Injection) ---
// Bu kısım, API'nin senin yazdığın katmanları tanımasını sağlar.
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddHttpClient<MatchService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger (Test ekranı) ayarları
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Match Analysis System API", Version = "v1" });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
var app = builder.Build();

// --- HTTP İSTEK KÖPRÜSÜ (Middleware) ---
if (app.Environment.IsDevelopment())
{
    // Proje çalıştığında test arayüzünü (Swagger) açar
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Match Analysis System v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
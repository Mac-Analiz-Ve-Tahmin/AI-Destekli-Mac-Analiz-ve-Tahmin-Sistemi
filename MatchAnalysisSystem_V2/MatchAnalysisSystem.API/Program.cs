using Microsoft.EntityFrameworkCore;
using MatchAnalysisSystem.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller ve API Desteðini Ekliyoruz
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Test arayüzü için Swagger'ý ekledik

// 2. DataAccess Katmanýndaki DbContext'i appsettings'teki adresle baðlýyoruz
builder.Services.AddDbContext<MatchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Business Servislerimizi sisteme kaydediyoruz
builder.Services.AddScoped<MatchAnalysisSystem.Business.Services.DataManagementManager>();
// Canlý veri servisimizi HttpClient desteðiyle kaydediyoruz
builder.Services.AddHttpClient<MatchAnalysisSystem.Business.Services.FootballApiService>();
var app = builder.Build();

// 3. Tarayýcýdan kolayca test edebilmek için Swagger Arayüzünü aktif ediyoruz
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();

app.UseStaticFiles();

app.Run();
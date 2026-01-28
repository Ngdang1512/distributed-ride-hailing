using RideHailing.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RegionService>();

var app = builder.Build();

// 2. Cấu hình Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- QUAN TRỌNG: CHO PHÉP CHẠY FILE HTML ---
app.UseDefaultFiles(); // Tự động tìm index.html
app.UseStaticFiles();  // Cho phép truy cập wwwroot
// -------------------------------------------

app.UseAuthorization();
app.MapControllers();

app.Run();
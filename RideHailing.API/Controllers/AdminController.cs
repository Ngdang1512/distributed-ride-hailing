using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideHailing.API.Services;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/web/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDatabaseFactory _dbFactory;

    public AdminController(IDatabaseFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // 1. API LẤY TẤT CẢ CHUYẾN ĐI THEO KHU VỰC (Dành cho Dashboard)
    [HttpGet("all-trips")]
    public async Task<IActionResult> GetAllTripsRegion([FromQuery] string region)
    {
        // Truyền tọa độ giả lập để mượn logic Factory bẻ lái đúng vùng
        double dummyLat = region.ToLower() == "north" ? 21.0 : 10.8;

        using var dbContext = _dbFactory.GetDbContext(dummyLat);
        var dbName = dbContext.Database.GetDbConnection().Database;

        var trips = await dbContext.Trips.OrderByDescending(t => t.CreatedAt).Take(50).ToListAsync();

        return Ok(new
        {
            Region = region.ToUpper(),
            ConnectedDB = dbName,
            TotalTrips = trips.Count,
            Trips = trips
        });
    }

    // 2. API KIỂM TRA SỨC KHỎE HỆ THỐNG (System Health Check)
    [HttpGet("health-check")]
    public IActionResult CheckHealth([FromQuery] string region)
    {
        double dummyLat = region.ToLower() == "north" ? 21.0 : 10.8;

        try
        {
            using var dbContext = _dbFactory.GetDbContext(dummyLat);
            var dbName = dbContext.Database.GetDbConnection().Database;
            bool isReplica = dbName.Contains("Replica");

            return Ok(new
            {
                Status = isReplica ? "WARNING" : "HEALTHY",
                Message = isReplica ? "Cảnh báo: Master đang sập, đang chạy trên Replica" : "Master hoạt động bình thường",
                ActiveDatabase = dbName,
                Region = region.ToUpper()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "CRITICAL",
                Message = "Toàn bộ cụm Server khu vực này đã sập!",
                Error = ex.Message
            });
        }
    }
    // 3. API BÁO CÁO THỐNG KÊ (Dùng để vẽ biểu đồ trên Web Admin)
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats([FromQuery] string region)
    {
        double dummyLat = region.ToLower() == "north" ? 21.0 : 10.8;

        using var dbContext = _dbFactory.GetDbContext(dummyLat);
        var dbName = dbContext.Database.GetDbConnection().Database;

        // Thống kê số lượng chuyến đi theo từng trạng thái (Pending, Cancelled, Completed)
        var tripStats = await dbContext.Trips
            .GroupBy(t => t.Status)
            .Select(g => new {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var totalTrips = tripStats.Sum(x => x.Count);

        return Ok(new
        {
            Region = region.ToUpper(),
            ConnectedDB = dbName,
            TotalTrips = totalTrips,
            Details = tripStats
        });
    }
}
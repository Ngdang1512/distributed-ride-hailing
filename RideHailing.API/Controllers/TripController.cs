using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideHailing.API.Data;
using RideHailing.API.Models;
using RideHailing.API.Services;

namespace RideHailing.API.Controllers;

[ApiController]
[Route("api/mobile/[controller]")]
public class TripController : ControllerBase
{
    private readonly IDatabaseFactory _dbFactory;

    public TripController(IDatabaseFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // 1. API ĐẶT CHUYẾN MỚI (Chỉ cho phép ghi trên Master)
    [HttpPost("book")]
    public async Task<IActionResult> BookTrip([FromBody] BookTripRequest request)
    {
        using var dbContext = _dbFactory.GetDbContext(request.PickupLat);
        dbContext.Database.EnsureCreated();

        var dbName = dbContext.Database.GetDbConnection().Database;

        // KIỂM TRA CHẾ ĐỘ READ-ONLY (Yêu cầu 4)
        if (dbName.Contains("Replica"))
        {
            return StatusCode(503, new
            {
                Success = false,
                Message = "Hệ thống đang bảo trì. Bạn chỉ có thể xem lịch sử, không thể đặt xe lúc này!",
                Database = dbName
            });
        }

        // Tạo User ảo nếu chưa có (Tránh lỗi khóa ngoại)
        var user = await dbContext.Users.FindAsync(request.UserId);
        if (user == null)
        {
            dbContext.Users.Add(new User { Id = request.UserId, FullName = "Khách Hàng " + request.UserId.ToString()[..4] });
            await dbContext.SaveChangesAsync();
        }

        var trip = new Trip { UserId = request.UserId, PickupLat = request.PickupLat, PickupLng = request.PickupLng, Status = "Pending" };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        return Ok(new { Success = true, Message = "Đặt xe thành công!", TripId = trip.Id, Database = dbName });
    }

    // 2. API XEM LỊCH SỬ (Cho phép đọc trên cả Master lẫn Replica)
    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetHistory(Guid userId, [FromQuery] double currentLat)
    {
        using var dbContext = _dbFactory.GetDbContext(currentLat);
        var dbName = dbContext.Database.GetDbConnection().Database;

        var trips = await dbContext.Trips
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(new { Success = true, Database = dbName, IsReadOnly = dbName.Contains("Replica"), Total = trips.Count, Data = trips });
    }

    // 3. API HỦY CHUYẾN (Chỉ cho phép cập nhật trên Master)
    [HttpPut("cancel/{tripId}")]
    public async Task<IActionResult> CancelTrip(Guid tripId, [FromQuery] double currentLat)
    {
        using var dbContext = _dbFactory.GetDbContext(currentLat);
        var dbName = dbContext.Database.GetDbConnection().Database;

        if (dbName.Contains("Replica"))
        {
            return StatusCode(503, new { Success = false, Message = "Hệ thống đang bảo trì, không thể cập nhật trạng thái chuyến đi!" });
        }

        var trip = await dbContext.Trips.FindAsync(tripId);
        if (trip == null) return NotFound(new { Success = false, Message = "Không tìm thấy chuyến đi" });

        trip.Status = "Cancelled";
        await dbContext.SaveChangesAsync();

        return Ok(new { Success = true, Message = "Đã hủy chuyến đi", Database = dbName });
    }
    // 4. API LẤY CHI TIẾT 1 CHUYẾN ĐI
    [HttpGet("detail/{tripId}")]
    public async Task<IActionResult> GetTripDetail(Guid tripId, [FromQuery] double currentLat)
    {
        // Vẫn hỗ trợ Read-Only: Nếu Master sập, API này tự động đọc từ Replica
        using var dbContext = _dbFactory.GetDbContext(currentLat);

        // Include cả thông tin User vào kết quả trả về
        var trip = await dbContext.Trips
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy dữ liệu chuyến đi!" });
        }

        return Ok(new
        {
            Success = true,
            ConnectedDB = dbContext.Database.GetDbConnection().Database,
            Data = trip
        });
    }
}

// Model nhận dữ liệu từ App
public class BookTripRequest
{
    public Guid UserId { get; set; }
    public double PickupLat { get; set; }
    public double PickupLng { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using RideHailing.Web.Services;
using RideHailing.Web.Entities;
using Microsoft.EntityFrameworkCore; // Cần dòng này để dùng .Count()

namespace RideHailing.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly RegionService _regionService;

        public BookingController(RegionService regionService)
        {
            _regionService = regionService;
        }

        // ==========================================
        // API 1: ĐẶT CHUYẾN XE (POST)
        // Dùng để App gọi đặt xe
        // ==========================================
        [HttpPost("book")]
        public IActionResult BookRide([FromBody] BookingRequest request)
        {
            // 1. Dựa vào vị trí khách -> Chọn Database (Bắc hay Nam)
            // Bắc: Lat > 17, Nam: Lat <= 17
            using var db = _regionService.GetDatabaseContext(request.Lat, request.Lng);

            try 
            {
                // 2. Tìm tài xế rảnh (IsAvailable = true) trong khu vực đó
                var availableDriver = db.Drivers
                    .FirstOrDefault(d => d.IsAvailable == true);

                if (availableDriver == null)
                {
                    return NotFound(new { Message = "Rất tiếc, không tìm thấy tài xế nào rảnh ở khu vực này!" });
                }

                // 3. Tạo chuyến đi mới
                var newTrip = new Trip
                {
                    TripId = Guid.NewGuid(),
                    DriverId = availableDriver.DriverId,
                    PickupLocation = request.PickupLocation,
                    Status = "Created",
                    CreatedDate = DateTime.Now
                };

                db.Trips.Add(newTrip);
                
                // 4. Cập nhật tài xế thành "Bận"
                availableDriver.IsAvailable = false;
                
                db.SaveChanges(); // Lưu thay đổi vào Database

                // 5. Trả về kết quả thành công
                return Ok(new { 
                    Message = "Đặt xe thành công!", 
                    TripId = newTrip.TripId, 
                    Driver = availableDriver.FullName,
                    Region = availableDriver.RegionCode
                });
            }
            catch (Exception ex)
            {
                // Bắt lỗi kết nối Database hoặc lỗi SQL
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==========================================
        // API 2: KIỂM TRA KẾT NỐI (GET) - QUAN TRỌNG
        // Dùng để kiểm tra xem Code có nối được vào SQL không
        // Link chạy: http://localhost:5213/api/Booking/test-connection
        // ==========================================
        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            var results = new List<string>();

            // --- TEST 1: KẾT NỐI MIỀN BẮC (Giả lập tọa độ 21.0) ---
            try 
            {
                using var dbHN = _regionService.GetDatabaseContext(21.0, 105.0);
                if (dbHN.Database.CanConnect())
                {
                    var driverCount = dbHN.Drivers.Count();
                    results.Add($"✅ MIỀN BẮC (HN): Kết nối THÀNH CÔNG! Tìm thấy {driverCount} tài xế trong bảng.");
                }
                else
                {
                    results.Add("❌ MIỀN BẮC (HN): Không thể kết nối (CanConnect = false).");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ MIỀN BẮC LỖI: {ex.Message}");
            }

            // --- TEST 2: KẾT NỐI MIỀN NAM (Giả lập tọa độ 10.0) ---
            try 
            {
                using var dbHCM = _regionService.GetDatabaseContext(10.0, 106.0);
                if (dbHCM.Database.CanConnect())
                {
                    var driverCount = dbHCM.Drivers.Count();
                    results.Add($"✅ MIỀN NAM (HCM): Kết nối THÀNH CÔNG! Tìm thấy {driverCount} tài xế trong bảng.");
                }
                else
                {
                    results.Add("❌ MIỀN NAM (HCM): Không thể kết nối (CanConnect = false).");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ MIỀN NAM LỖI: {ex.Message}");
            }

            return Ok(results);
        }
    }

    // Class nhận dữ liệu từ giao diện gửi lên
    public class BookingRequest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string PickupLocation { get; set; }
    }
}
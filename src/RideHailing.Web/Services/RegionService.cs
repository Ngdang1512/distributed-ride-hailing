using Microsoft.EntityFrameworkCore;
using RideHailing.Web.Data;

namespace RideHailing.Web.Services
{
    public class RegionService
    {
        private readonly IConfiguration _configuration;

        public RegionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Hàm quyết định kết nối dựa trên vị trí (Lat/Lon)
        public RideHailingContext GetDatabaseContext(double lat, double lon)
        {
            string connectionString;

            // Logic giả lập: Vĩ độ > 17 là Miền Bắc (Hà Nội), còn lại là Miền Nam
            if (lat > 17.0) 
            {
                // Lấy chuỗi kết nối HanoiConnection từ appsettings.json
                connectionString = _configuration.GetConnectionString("HanoiConnection");
            }
            else 
            {
                // Lấy chuỗi kết nối HcmConnection
                connectionString = _configuration.GetConnectionString("HcmConnection");
            }

            // Tạo DbContext với chuỗi kết nối vừa chọn
            var optionsBuilder = new DbContextOptionsBuilder<RideHailingContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new RideHailingContext(optionsBuilder.Options);
        }
    }
}
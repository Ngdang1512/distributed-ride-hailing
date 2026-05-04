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

        public RideHailingContext GetDatabaseContext(double lat, double lon)
        {
            // Vĩ độ > 17 là Miền Bắc (Hà Nội), còn lại là Miền Nam[cite: 6]
            string masterConn = lat > 17.0
                ? _configuration.GetConnectionString("HanoiConnection")!
                : _configuration.GetConnectionString("HcmConnection")!;

            string replicaConn = lat > 17.0
                ? _configuration.GetConnectionString("HanoiReplica")!
                : _configuration.GetConnectionString("HcmReplica")!;

            var optionsBuilder = new DbContextOptionsBuilder<RideHailingContext>();
            optionsBuilder.UseSqlServer(masterConn);
            var context = new RideHailingContext(optionsBuilder.Options);

            // Thử kết nối Master, nếu sống thì dùng
            if (context.Database.CanConnect())
            {
                return context;
            }

            // Nếu Master chết, tự động bẻ lái sang Replica
            var replicaOptions = new DbContextOptionsBuilder<RideHailingContext>();
            replicaOptions.UseSqlServer(replicaConn);
            return new RideHailingContext(replicaOptions.Options);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using RideHailing.Web.Entities;

namespace RideHailing.Web.Data
{
    public class RideHailingContext : DbContext
    {
        // Constructor nhận vào Options (chứa Connection String)
        public RideHailingContext(DbContextOptions<RideHailingContext> options) : base(options)
        {
        }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
    }
}
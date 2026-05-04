using Microsoft.EntityFrameworkCore;
using RideHailing.API.Models;

namespace RideHailing.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Trip> Trips { get; set; }
}
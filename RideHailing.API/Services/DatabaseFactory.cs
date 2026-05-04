using Microsoft.EntityFrameworkCore;
using RideHailing.API.Data;

namespace RideHailing.API.Services;

public interface IDatabaseFactory
{
    AppDbContext GetDbContext(double latitude);
}

public class DatabaseFactory : IDatabaseFactory
{
    private readonly IConfiguration _configuration;

    public DatabaseFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AppDbContext GetDbContext(double latitude)
    {
        string masterConn = latitude > 16.0
            ? _configuration.GetConnectionString("North_Master")!
            : _configuration.GetConnectionString("South_Master")!;

        string replicaConn = latitude > 16.0
            ? _configuration.GetConnectionString("North_Replica")!
            : _configuration.GetConnectionString("South_Replica")!;

        var masterOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(masterConn).Options;
        var context = new AppDbContext(masterOptions);

        if (context.Database.CanConnect()) return context;

        // Failover
        var replicaOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(replicaConn).Options;
        return new AppDbContext(replicaOptions);
    }
}
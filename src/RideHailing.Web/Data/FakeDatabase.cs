using RideHailing.Web.Models;

namespace RideHailing.Web.Data;

public static class FakeDatabase
{
    // Mỗi region là một "node" riêng
    public static List<Trip> HCM = new();
    public static List<Trip> HN = new();
    public static List<Trip> DN = new();

    // Lấy danh sách trip theo Region (ENUM)
    public static List<Trip> GetTripsByRegion(Region region)
    {
        return region switch
        {
            Region.HCM => HCM,
            Region.HN => HN,
            Region.DN => DN,
            _ => HCM
        };
    }

    // Thêm trip đúng node
    public static void AddTrip(Trip trip)
    {
        GetTripsByRegion(trip.Region).Add(trip);
    }

    public static List<Trip> GetAllTrips()
    {
        return HCM
            .Concat(HN)
            .Concat(DN)
            .ToList();
    }
}

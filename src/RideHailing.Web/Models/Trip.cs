namespace RideHailing.Web.Models;

public class Trip
{
    public long Id { get; set; }

    public string PickupLocation { get; set; } = "";
    public string DropoffLocation { get; set; } = "";

    public Region Region { get; set; }

    public TripStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Driver? Driver { get; set; }

    public int Duration { get; set; }
    public decimal Fare { get; set; }
}

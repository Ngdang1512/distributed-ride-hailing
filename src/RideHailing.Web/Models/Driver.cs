namespace RideHailing.Web.Models;

public class Driver
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string VehicleNumber { get; set; } = "";
    public Region Region { get; set; }
}

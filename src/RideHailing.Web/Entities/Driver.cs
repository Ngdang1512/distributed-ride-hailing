using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RideHailing.Web.Entities
{
    public class Driver
    {
        [Key]
        public Guid DriverId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string VehicleType { get; set; } // Bike, Car
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public bool IsAvailable { get; set; }
        public string RegionCode { get; set; } // "HN" hoặc "HCM"
    }
}
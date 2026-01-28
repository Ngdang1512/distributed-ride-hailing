using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RideHailing.Web.Entities
{
    public class Trip
    {
        [Key]
        public Guid TripId { get; set; }
        public Guid DriverId { get; set; }
        // public Guid UserId { get; set; } // Giả lập user
        public string PickupLocation { get; set; }
        // public string DropoffLocation { get; set; }
        public string Status { get; set; } // Created, Accepted, Completed
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
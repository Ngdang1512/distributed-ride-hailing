namespace RideHailing.Web.Models
{
    public enum TripStatus
    {
        Searching,        // Đang tìm tài xế
        DriverAssigned,   // Đã có tài xế
        Accepted,         // Tài xế nhận cuốc
        Completed,        // Hoàn thành
        Cancelled         // Huỷ
    }
}

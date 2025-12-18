namespace BookingSystem.Core.Constants
{
    public class CreateBookingDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
    }
}

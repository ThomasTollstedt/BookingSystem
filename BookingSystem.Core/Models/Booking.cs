using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Core.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int RoomId { get; set; }
        public Room Room { get;  set; }

        public int UserId { get; set; }
        public User User { get;  set; }


        public Booking() { }
        

        public Booking(DateTime startTime, DateTime endTime, Room room, User user)
        {
            if (endTime < startTime)
            {
                throw new ArgumentOutOfRangeException(nameof(endTime), "End time must be after start time.");
            }

            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), "A room must be selected");
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "A user must be selected");
            }

            StartTime = startTime;
            EndTime = endTime;
            Room = room;
            RoomId = room.Id;
            User = user;
            UserId = user.Id;


        }

    }
}

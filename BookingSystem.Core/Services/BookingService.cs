using BookingSystem.Core.Interfaces;
using BookingSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Core.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepo;
        public BookingService(IBookingRepository bookingRepo) => _bookingRepo = bookingRepo;


        public async Task AddBookingAsync(Booking booking)
        {
            bool IsRoomAvailable = await IsRoomAvailableAsync(booking.RoomId, booking.StartTime, booking.EndTime);

            if (!IsRoomAvailable)
            {
                throw new InvalidOperationException("The room is already booked for the selected time range.");
            }

  

            await _bookingRepo.AddAsync(booking);
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime)
        {
            var roomBookings = await _bookingRepo.GetBookingsByRoomAsync(roomId);


            // Any() returnerar true om det FINNS en krock.
            bool hasConflict = roomBookings.Any(b => b.RoomId == roomId &&
                                            startTime < b.EndTime &&
                                            endTime > b.StartTime);

            // Om det finns en krock, dvs hasConflict == true, så är rummet INTE tillgängligt, returnera false.
            return !hasConflict;

        }

        //public async Task<List<Booking>> GetAllBookingsAsync()
        //{
        //    var allBookings = await _bookingRepo.();
        //}

        public async Task<List<Booking>> GetBookingsForRoomAsync(int roomId)
        {
            var listOfBookings = await _bookingRepo.GetBookingsByRoomAsync(roomId);
            return listOfBookings;
        }
    }
}

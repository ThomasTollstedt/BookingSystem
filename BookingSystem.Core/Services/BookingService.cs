using BookingSystem.Core.Constants;
using BookingSystem.Core.Interfaces;
using BookingSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        public BookingService(IBookingRepository bookingRepo) => _bookingRepo = bookingRepo;


        public async Task<Booking> AddBookingAsync(CreateBookingDto dto)
        {
            var room = await _bookingRepo.GetRoomByIdAsync(dto.RoomId);
            var user = await _bookingRepo.GetUserByIdAsync(dto.UserId);

            if (room == null)
            {
                throw new KeyNotFoundException($"Room with ID {dto.RoomId} not found.");
            }
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");
            }


            bool IsRoomAvailable = await IsRoomAvailableAsync(dto.RoomId, dto.StartTime, dto.EndTime);

            if (!IsRoomAvailable)
            {
                throw new InvalidOperationException("The room is already booked for the selected time range.");
            }

            var booking = new Booking(dto.StartTime, dto.EndTime, room, user);

            await _bookingRepo.AddAsync(booking);
                
            return booking;
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


        public async Task<List<Booking>> GetBookingsForRoomAsync(int roomId)
        {
            var listOfBookings = await _bookingRepo.GetBookingsByRoomAsync(roomId);
            return listOfBookings;
        }
    }
}

using BookingSystem;
using BookingSystem.Core.Constants;
using BookingSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Core.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> AddBookingAsync(CreateBookingDto dto);
        Task<List<Booking>> GetBookingsForRoomAsync(int roomId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime);
    }
}

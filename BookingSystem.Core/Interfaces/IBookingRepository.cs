using BookingSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Core.Interfaces
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking);

        Task<List<Booking>> GetBookingsByRoomAsync(int roomId);

        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<User?> GetUserByIdAsync(int userId);
    }
}

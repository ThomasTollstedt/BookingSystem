using BookingSystem.Core.Interfaces;
using BookingSystem.Core.Models;
using BookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace BookingSystem.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context) => _context = context;


        public async Task AddAsync(Booking booking)
        {
            var newBooking = await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Booking>> GetBookingsByRoomAsync(int roomId)
        {
            var booking = await _context.Bookings
                .Where(b => b.RoomId == roomId)
                .ToListAsync();

            return booking;
        }
    }
}

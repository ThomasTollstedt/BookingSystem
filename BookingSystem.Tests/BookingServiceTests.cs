using BookingSystem.Core.Models;
using BookingSystem.Core.Services;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookingSystem.Tests
{
    public class BookingServiceTests
    {
        private readonly BookingDbContext _context;
        private readonly BookingRepository _bookingRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BookingDbContext(options);
            _bookingRepository = new BookingRepository(_context);
            _bookingService = new BookingService(_bookingRepository);
        }


        [Fact]
        public void AddBooking_EndTimeBeforeStartTime_ThrowException()
        {
            //Arrange
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(-3);
            Room room = new Room();

            //Act + Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new Booking(startTime, endTime, room, null));

        }

        [Fact]
        public void Booking_WithoutAssignedRoom_ThrowException()
        {
            //Arrange
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(3);
            User user = new User();
            //Act + Assert        
            Assert.Throws<ArgumentNullException>(() => new Booking(startTime, endTime, null, user));
        }


        [Fact]
        public void Booking_WithoutAssignedUser_ThrowException()
        {
            //Arrange
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(3);
            Room room = new Room();
            //act + Assert
            Assert.Throws<ArgumentNullException>(() => new Booking(startTime, endTime, room, null));
        }


        [Fact]
        public async Task Booking_AddBooking_Success()
        {
            //Arrange
            Booking booking = new Booking(DateTime.Now, DateTime.Now.AddHours(2), new Room(), new User());
            //Act 
            await _bookingService.AddBookingAsync(booking);
            //Assert
            var allBookings = await _context.Bookings.ToListAsync();
            Assert.Contains(booking, allBookings);
            Assert.NotEqual(0, booking.Id);
        }

        [Fact]
        public async Task AddBooking_OverlappingBooking_ThrowException()
        {
            //arrange
            Booking existingBooking = new Booking(DateTime.Now, DateTime.Now.AddHours(2), new Room(), new User());
            await _bookingService.AddBookingAsync(existingBooking);
            
            //Act + Assert
            Booking newBooking = new Booking(DateTime.Now.AddHours(1), DateTime.Now.AddHours(3), existingBooking.Room, new User());
            await Assert.ThrowsAsync<InvalidOperationException>(() => _bookingService.AddBookingAsync(newBooking));

        }

        [Fact]
        public async Task IsRoomAvailable_RoomIsBooked_ReturnFalse()
        {
            //Arrange
            Booking existingBooking = new Booking(DateTime.Now, DateTime.Now.AddHours(2), new Room(), new User());
            await _bookingService.AddBookingAsync(existingBooking);
            
            //Act
            bool RoomIsAvailable = await _bookingService.IsRoomAvailableAsync(existingBooking.RoomId, DateTime.Now.AddMinutes(30), DateTime.Now.AddHours(1));

            // Assert
            Assert.False(RoomIsAvailable);
        }

        [Fact]
        public async Task IsRoomAvailable_RoomIsNotBooked_ReturnTrue()
        {
            //Arrange 
            Booking existingBooking = new Booking(DateTime.Now, DateTime.Now.AddHours(2), new Room(), new User());
            await _bookingService.AddBookingAsync(existingBooking);
            //Act
            bool RoomIsAvailable = await _bookingService.IsRoomAvailableAsync(existingBooking.RoomId, DateTime.Now.AddHours(3), DateTime.Now.AddHours(4));

            //Assert    
            Assert.True(RoomIsAvailable);

        }
        // TA BORT  !!!! 

        //[Fact]
        //public async Task GetAllBookings_ReturnListOfBookings()
        //{
        //    //Arrange   
        //    Booking booking1 = new Booking(DateTime.Now, DateTime.Now.AddHours(2), new Room(), new User());
        //    Booking booking2 = new Booking(DateTime.Now.AddHours(3), DateTime.Now.AddHours(5), new Room(), new User());
        //    Booking booking3 = new Booking(DateTime.Now.AddHours(6), DateTime.Now.AddHours(8), new Room(), new User());
        //    await _bookingService.AddBookingAsync(booking1);
        //    await _bookingService.AddBookingAsync(booking2);
        //    await _bookingService.AddBookingAsync(booking3);
        //    //Act
        //    var allBookings = await _bookingService.GetAllBookings();

        //    //Assert
        //    Assert.Equal(3, allBookings.Count);
        //}

        [Fact]
        public async Task GetBookings_SpecificRoom_ReturnListOfBookings()
        {
            //Arrange
            
            Room roomA = new Room();
            Room roomB = new Room();
            Booking booking1 = new Booking(DateTime.Now, DateTime.Now.AddHours(2), roomA, new User());
            Booking booking2 = new Booking(DateTime.Now.AddHours(3), DateTime.Now.AddHours(5), roomA, new User());
            Booking booking3 = new Booking(DateTime.Now.AddHours(6), DateTime.Now.AddHours(8), roomB, new User());
            await _bookingService.AddBookingAsync(booking1);
            await _bookingService.AddBookingAsync(booking2);
            await _bookingService.AddBookingAsync(booking3);
            //Act
            var bookingsRoomA = await _bookingService.GetBookingsForRoomAsync(roomA.Id);
            //Assert
            Assert.Equal(2, bookingsRoomA.Count);


        }



    }

}

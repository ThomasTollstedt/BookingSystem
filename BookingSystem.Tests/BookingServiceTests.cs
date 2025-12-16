using BookingSystem.Core.Constants;
using BookingSystem.Core.Interfaces;
using BookingSystem.Core.Models;
using BookingSystem.Core.Services;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;

namespace BookingSystem.Tests
{
    public class BookingServiceTests
    {
        private readonly BookingDbContext _context;
        private readonly BookingRepository _bookingRepository;
        private readonly Mock<IBookingRepository> _mockBookingService;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            //var options = new DbContextOptionsBuilder<BookingDbContext>()
            //    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            //    .Options;

            //_context = new BookingDbContext(options);
            //_bookingRepository = new BookingRepository(_context);
            _mockBookingService = new Mock<IBookingRepository>();
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
           
            var room = new Room();
            var user = new User();
            _context.Add(room);
            _context.Add(user);
            _context.SaveChanges();

            var booking = new CreateBookingDto
            { 
            RoomId = room.Id,
            UserId = user.Id,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2)

            };

            //Act 
           var createdBooking = await _bookingService.AddBookingAsync(booking);
            //Assert
            var allBookings = await _context.Bookings.ToListAsync();
            Assert.Contains(createdBooking, allBookings);
            Assert.NotEqual(0, createdBooking.Id);
        }

        [Fact]
        public async Task AddBooking_OverlappingBooking_ThrowException()
        {
            //arrange
            
            var room = new Room();
            var user = new User();
            _context.Add(room);
            _context.Add(user);
            _context.SaveChanges();

            var booking = new CreateBookingDto
            {
                RoomId = room.Id,
                UserId = user.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)

            };

            //Act ++ Assrrty
            var oldBooking = await _bookingService.AddBookingAsync(booking);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _bookingService.AddBookingAsync(new CreateBookingDto
            {
                RoomId = room.Id,
                UserId = user.Id,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            }));


        }

        [Fact]
        public async Task IsRoomAvailable_RoomIsBooked_ReturnFalse()
        {
            //Arrange
            var room = new Room();
            var user = new User();
            _context.Add(room);
            _context.Add(user);
            _context.SaveChanges();

            var booking = new CreateBookingDto
            {
                RoomId = room.Id,
                UserId = user.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)

            };

            var createdBooking = await _bookingService.AddBookingAsync(booking);
            
            //Act
            bool RoomIsAvailable = await _bookingService.IsRoomAvailableAsync(createdBooking.RoomId, DateTime.Now.AddMinutes(30), DateTime.Now.AddHours(1));

            // Assert
            Assert.False(RoomIsAvailable);
        }

        [Fact]
        public async Task IsRoomAvailable_RoomIsNotBooked_ReturnTrue()
        {
            //Arrange 
            var room = new Room();
            var user = new User();
            _context.Add(room);
            _context.Add(user);
            _context.SaveChanges();

            var booking = new CreateBookingDto
            {
                RoomId = room.Id,
                UserId = user.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)

            };

            var createdBooking = await _bookingService.AddBookingAsync(booking);

            //Act
            bool RoomIsAvailable = await _bookingService.IsRoomAvailableAsync(createdBooking.RoomId, DateTime.Now.AddHours(3), DateTime.Now.AddHours(4));

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
            //Arrange Moq
            var roomAId = 1;
            var roomBId = 2;
            var userId = 1;

            var roomA = new Room { Id = roomAId };
            var roomB = new Room { Id = roomBId };
            var user = new User { Id = userId };

            var expectedBookingsRoomA = new List<Booking> 
                           {
                new Booking(DateTime.Now, DateTime.Now.AddHours(2), roomA, user),
                new Booking(DateTime.Now.AddHours(3), DateTime.Now.AddHours(5), roomA, user) { Id = 2 }
            };

            var mockBookingRepository = new Mock<IBookingRepository>();
            mockBookingRepository
                .Setup(repo => repo.GetBookingsByRoomAsync(roomAId))
                .ReturnsAsync(expectedBookingsRoomA);

            var bookingService = new BookingService(mockBookingRepository.Object);

            //Act
            var bookingsRoomA = await bookingService.GetBookingsForRoomAsync(roomAId);

            Assert.Equal(2, bookingsRoomA.Count);
            mockBookingRepository.Verify(r => r.GetBookingsByRoomAsync(roomAId), Times.Once);


            //Arrange felaktigt nedan

            //var roomA = new Room();
            //var roomB = new Room();
            //var user = new User();
            //_context.Add(roomA);
            //_context.Add(roomB);
            //_context.Add(user);
            //_context.SaveChanges();


            //var booking1 = new CreateBookingDto
            //{
            //    RoomId = roomA.Id,
            //    UserId = user.Id,
            //    StartTime = DateTime.Now,
            //    EndTime = DateTime.Now.AddHours(2)

            //};

            //var booking2 = new CreateBookingDto
            //{
            //    RoomId = roomA.Id,
            //    UserId = user.Id,
            //    StartTime = DateTime.Now.AddHours(3),
            //    EndTime = DateTime.Now.AddHours(5)

            //};
            //var booking3 = new CreateBookingDto
            //{
            //    RoomId = roomB.Id,
            //    UserId = user.Id,
            //    StartTime = DateTime.Now.AddHours(6),
            //    EndTime = DateTime.Now.AddHours(8)

            //};

            //var booking1 = new CreateBookingDto(DateTime.Now, DateTime.Now.AddHours(2), roomA, user);
            //Booking booking2 = new Booking(DateTime.Now.AddHours(3), DateTime.Now.AddHours(5), roomA, user);
            //Booking booking3 = new Booking(DateTime.Now.AddHours(6), DateTime.Now.AddHours(8), roomB, user);
            //await _bookingService.AddBookingAsync(booking1);
            //await _bookingService.AddBookingAsync(booking2);
            //await _bookingService.AddBookingAsync(booking3);
            ////Act
            //var bookingsRoomA = await _bookingService.GetBookingsForRoomAsync(roomA.Id);
            ////Assert
            //Assert.Equal(2, bookingsRoomA.Count);


        }



    }

}

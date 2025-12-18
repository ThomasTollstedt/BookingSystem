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


        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _bookingService = new BookingService(_mockBookingRepository.Object);
        }


        [Fact]
        public void AddBooking_EndTimeBeforeStartTime_ThrowException() //Överlappande bokning
        {
            //Arrange
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(-3);
            Room room = new Room();

            //Act + Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new Booking(startTime, endTime, room, null));

        }

        [Fact]
        public void Booking_WithoutAssignedRoom_ThrowException() //Bokning utan rum
        {
            //Arrange
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(3);
            User user = new User();
            //Act + Assert        
            Assert.Throws<ArgumentNullException>(() => new Booking(startTime, endTime, null, user));
        }


        [Fact]
        public void Booking_WithoutAssignedUser_ThrowException() //Bokning utan användare
        {
            //Arrange
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddHours(3);
            Room room = new Room();
            //act + Assert
            Assert.Throws<ArgumentNullException>(() => new Booking(startTime, endTime, room, null));
        }


        [Fact]
        public async Task Booking_AddBooking_Success() //Skapa en lyckad bokning 
        {
            //Arrange

            int roomId = 1;
            int userId = 1;
            var room = new Room { Id = roomId };
            var user = new User { Id = userId };

            var bookingDto = new CreateBookingDto
            {
                RoomId = roomId,
                UserId = userId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            _mockBookingRepository
                .Setup(repo => repo.GetRoomByIdAsync(roomId))
                .ReturnsAsync(room);

            _mockBookingRepository
                .Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            _mockBookingRepository
                .Setup(repo => repo.GetBookingsByRoomAsync(roomId))
                .ReturnsAsync(new List<Booking>()); //ingen överlappande bokning

            _mockBookingRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            //Act 
            var createdBooking = await _bookingService.AddBookingAsync(bookingDto);
            //Assert
            Assert.NotNull(createdBooking); // Simpel check att bokningen skapades men inte mycket mer än så. 
            _mockBookingRepository.Verify(repo => repo.AddAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task AddBooking_OverlappingBooking_ThrowException() //Överlappande bokning nekas
        {
            //Arrange

            int roomId = 1;
            int userId = 1;
            var room = new Room { Id = roomId };
            var user = new User { Id = userId };

            var startTime = DateTime.Now;

            var existingBooking = new Booking(startTime, startTime.AddHours(2), room, user)
            {
                Id = 1,
                RoomId = roomId,

            };


            //Mockar rum och user
            _mockBookingRepository
                .Setup(repo => repo.GetRoomByIdAsync(roomId))
                .ReturnsAsync(room);

            _mockBookingRepository
                .Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);


            _mockBookingRepository
                .Setup(repo => repo.GetBookingsByRoomAsync(roomId))
                .ReturnsAsync(new List<Booking> { existingBooking }); //Returnerar en redan existerande bokning som överlappar

            //Act
            var overlappingBookingDto = new CreateBookingDto
            {
                RoomId = roomId,
                UserId = userId,
                StartTime = startTime.AddHours(1), //Överlappande tid mot firstBookingDto
                EndTime = startTime.AddHours(3)
            };

            //Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _bookingService.AddBookingAsync(overlappingBookingDto));
            // Verify AddAsync was NEVER called (conflict detected before save)
            _mockBookingRepository.Verify(repo => repo.AddAsync(It.IsAny<Booking>()), Times.Never);

        }

        [Fact]
        public async Task IsRoomAvailable_RoomIsBooked_ReturnFalse()
        {
            //Arrange
            int roomId = 1;
            int userId = 1;
            var room = new Room { Id = roomId };
            var user = new User { Id = userId};
           
            var startTime = DateTime.Now;

            //skapar en bokning som redan finns i systemet
            var existingBooking = new Booking(startTime, startTime.AddHours(2), room, user)
            {
                Id = 1,
                RoomId = roomId,
            };

           _mockBookingRepository
                .Setup(repo => repo.GetBookingsByRoomAsync(roomId))
                .ReturnsAsync(new List<Booking> { existingBooking }); //Returnerar en redan existerande bokning

            //Act
            // Se över om tillgänglighet fungerar korrekt tidigare bokning nu -> nu+2h, nya bokningen 30min -> 1h
            bool IsRoomAvailable = await _bookingService.IsRoomAvailableAsync(
                roomId, 
                startTime.AddMinutes(30), 
                startTime.AddHours(1));

            // Assert
            Assert.False(IsRoomAvailable); //Förväntar att rummet inte är tillgängligt
        }

        [Fact]
        public async Task IsRoomAvailable_RoomIsNotBooked_ReturnTrue()
        {
     
            //Arrange 
            var roomId = 1; int userId = 1;
            var startTime = DateTime.Now;

            var existingBooking = new Booking(startTime, startTime.AddHours(2), new Room { Id = roomId }, new User { Id = 1 })
            {
                RoomId = roomId,
                UserId = userId
            };

            
            _mockBookingRepository
                .Setup(repo => repo.GetBookingsByRoomAsync(roomId))
                .ReturnsAsync(new List<Booking> { existingBooking });

            //Act
            bool isRoomAvailable = await _bookingService.IsRoomAvailableAsync(
                roomId,
                startTime.AddHours(3),
                startTime.AddHours(4));

            //Assert
            Assert.True(isRoomAvailable);
            
        }


        [Fact]
        public async Task GetBookings_SpecificRoom_ReturnListOfBookings()
        {
            //Arrange 
            var roomAId = 1;
            var userId = 1;
            var roomA = new Room { Id = roomAId };
            var user = new User { Id = userId };

            var expectedBookingsRoomA = new List<Booking>
                           {
                new Booking(DateTime.Now, DateTime.Now.AddHours(2), roomA, user),
                new Booking(DateTime.Now.AddHours(3), DateTime.Now.AddHours(5), roomA, user) { Id = 2 }
            };

            
            _mockBookingRepository
                .Setup(repo => repo.GetBookingsByRoomAsync(roomAId))
                .ReturnsAsync(expectedBookingsRoomA);

            

            //Act
            var result = await _bookingService.GetBookingsForRoomAsync(roomAId);
            // Assert
            Assert.Equal(2, result.Count);
            _mockBookingRepository.Verify(r => r.GetBookingsByRoomAsync(roomAId), Times.Once);

        }



    }

}

using BookingSystem.Core.Constants;
using BookingSystem.Core.Models;
using BookingSystem.Infrastructure.Data;
using BookingSystemWebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace BookingSystem.IntegrationTests
{
    public class BookingSystemWebApiTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _webAppFactory;
        private readonly HttpClient _httpClient;

        private int _createdRoomId;
        private int _createdUserId;

        public BookingSystemWebApiTests()
        {
            _webAppFactory = new WebApplicationFactory<BookingSystemWebApi.Program>();
            _httpClient = _webAppFactory.CreateDefaultClient();
        }

        //xUnit kör denna innan varje test
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        // xUnit kör denna efter varje test, även om testerna misslyckas
        public async Task DisposeAsync()
        {
            using (var scope = _webAppFactory.Services.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

                // 1. Remove bookings for our test entities
                var bookingsToDelete = scopedContext.Bookings
                    .Where(b => b.RoomId == _createdRoomId || b.UserId == _createdUserId)
                    .ToList();
                scopedContext.Bookings.RemoveRange(bookingsToDelete);

                // 2. Remove the test room
                var room = await scopedContext.Rooms.FindAsync(_createdRoomId);
                if (room != null) scopedContext.Rooms.Remove(room);

                // 3. Remove the test user
                var user = await scopedContext.Users.FindAsync(_createdUserId);
                if (user != null) scopedContext.Users.Remove(user);

                await scopedContext.SaveChangesAsync();
            }
        }



        [Fact]
        public async Task AddBookingToDb()
        {
            
            //Arrange

            using (var scope = _webAppFactory.Services.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var room = new Room();
                var user = new User();
                scopedContext.Add(room);
                scopedContext.Add(user);
                await scopedContext.SaveChangesAsync();

                _createdRoomId = room.Id;
                _createdUserId = user.Id;
            }

            var dto = new CreateBookingDto
            {
                RoomId = _createdRoomId,
                UserId = _createdUserId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };

            //Act
            var response = await _httpClient.PostAsJsonAsync("/api/BookingSystem", dto);

            //Assert    
            response.EnsureSuccessStatusCode(); // Checka för 200 OK
            var stringResult = await response.Content.ReadAsStringAsync();
            Assert.Contains($"{_createdRoomId}", stringResult);

            
            }

        
    

        [Fact]
        public async Task GetBookingsFromDb_ReturnsBooking()
        {
            //Arrange

            using (var scope = _webAppFactory.Services.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var room = new Room();
                var user = new User();
                var booking = new Booking
                {
                    Room = room,
                    User = user,
                    StartTime = DateTime.Now.AddDays(1),
                    EndTime = DateTime.Now.AddDays(1).AddHours(2)

                };
                scopedContext.Add(room);
                scopedContext.Add(user);
                scopedContext.Add(booking);
                await scopedContext.SaveChangesAsync();

                _createdRoomId = room.Id;
                _createdUserId = user.Id;
            }

            //Act 
            var response = await _httpClient.GetAsync($"/api/BookingSystem?roomId={_createdRoomId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();

            Assert.NotNull(bookings);
            Assert.Contains(bookings, b => b.RoomId == _createdRoomId && b.UserId == _createdUserId);

        }

      
    }
}
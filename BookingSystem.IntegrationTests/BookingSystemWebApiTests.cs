using BookingSystem.Core.Constants;
using BookingSystem.Core.Models;
using BookingSystem.Infrastructure.Data;
using BookingSystemWebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace BookingSystem.IntegrationTests
{
    public class BookingSystemWebApiTests
    {
        private readonly WebApplicationFactory<Program> _webAppFactory;
        private readonly HttpClient _httpClient;

        public BookingSystemWebApiTests()
        {
            _webAppFactory = new WebApplicationFactory<BookingSystemWebApi.Program>();
            _httpClient = _webAppFactory.CreateDefaultClient();
        }



        [Fact]
        public async Task AddBookingToDb()
        {
            int roomId;
            int userId;


            using (var scope = _webAppFactory.Services.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var room = new Room();
                var user = new User();
                scopedContext.Add(room);
                scopedContext.Add(user);
                await scopedContext.SaveChangesAsync();

                roomId = room.Id;
                userId = user.Id;


            }
            var dto = new CreateBookingDto
            {
                RoomId = roomId,
                UserId = userId,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };



            var response = await _httpClient.PostAsJsonAsync("/api/BookingSystem", dto);
            var stringResult = await response.Content.ReadAsStringAsync();

            Assert.Contains($"{roomId}", stringResult);

            //Clean up
            using (var scope = _webAppFactory.Services.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

                //remove the booking that was created
                var bookingsToDelete = scopedContext.Bookings
                    .Where(b => b.RoomId == roomId && b.UserId == userId)
                    .ToList();
                scopedContext.Bookings.RemoveRange(bookingsToDelete);

                // Remove the test room and user
                var room = scopedContext.Rooms.Find(roomId);
                var user = scopedContext.Users.Find(userId);
                if (room != null) scopedContext.Rooms.Remove(room);
                if (user != null) scopedContext.Users.Remove(user);

                await scopedContext.SaveChangesAsync();


            }

        }
        public void Dispose()
        {
            _httpClient?.Dispose();
            _webAppFactory?.Dispose();
        }

        [Fact]
        public async Task GetBookingsFromDb_ReturnsBooking()
        {
            int roomId;
            int userId;


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

                roomId = room.Id;
                userId = user.Id;

            }

            //Act 
            var response = await _httpClient.GetAsync($"/api/BookingSystem?roomId={roomId}");

            //Assert
            response.EnsureSuccessStatusCode();
            var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();

            Assert.NotNull(bookings);
            Assert.Contains(bookings, b => b.RoomId == roomId && b.UserId == userId);

            // Cleanup
            using (var scope = _webAppFactory.Services.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                var bookingsToDelete = scopedContext.Bookings.Where(b => b.RoomId == roomId).ToList();
                scopedContext.Bookings.RemoveRange(bookingsToDelete);

                var room = scopedContext.Rooms.Find(roomId);
                var user = scopedContext.Users.Find(userId);
                if (room != null) scopedContext.Rooms.Remove(room);
                if (user != null) scopedContext.Users.Remove(user);

                await scopedContext.SaveChangesAsync();
            }
        }
    }
}
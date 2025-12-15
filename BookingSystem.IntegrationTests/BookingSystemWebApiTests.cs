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
                scopedContext.SaveChanges();

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

            

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7179/api/BookingSystem", dto);
            var stringResult = await response.Content.ReadAsStringAsync();

            Assert.Contains($"{roomId}", stringResult);
        
        }


    }
}

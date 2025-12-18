using BookingSystem.Core.Constants;
using BookingSystem.Core.Interfaces;
using BookingSystem.Core.Models;

using Microsoft.AspNetCore.Mvc;

namespace BookingSystemWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingSystemController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingSystemController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        //GET /api/bookings
        [HttpGet]
        public async Task<IActionResult> GetAllBookingsForRoom(int roomId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsForRoomAsync(roomId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest($"Det gick inte att hämta bokningarna. {ex.Message}");
            }
        }

        //POST /api/bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking (CreateBookingDto dto)
        {
            try
            {
               var createdBooking = await _bookingService.AddBookingAsync(dto);
                return Ok(createdBooking);
            }
            catch (Exception ex)
            {
                return BadRequest($"Det gick inte att lägga till bokningen. {ex.Message}");
            }



        }
}
}

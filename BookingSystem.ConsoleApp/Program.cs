using BookingSystem.Core.Constants;
using BookingSystem.Core.Models;
using System.Net.Http.Json;

namespace BookingSystem.ConsoleApp
{
    internal class Program
    {
        private static readonly string ApiBaseUrl = "https://localhost:7179/api/BookingSystem";
        private static readonly HttpClient _httpClient = new HttpClient();
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- BookingSystem Client ---");

            bool running = true;
            while (running)
            {
                Console.WriteLine("\nVälj ett alternativ:");
                Console.WriteLine("1. Skapa bokning");
                Console.WriteLine("2. Visa bokningar för ett rum");
                Console.WriteLine("3. Kontrollera tillgänglighet");
                Console.WriteLine("4. Avsluta");
                Console.Write("Val: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await CreateBookingAsync();
                            break;
                        case "2":
                            await ListBookingsAsync();
                            break;
                        case "3":
                            await CheckAvailabilityAsync();
                            break;
                        case "4":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Ogiltigt val. Försök igen.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ett oväntat fel inträffade: {ex.Message}");

                }
            }
        }

        private static async Task CheckAvailabilityAsync()
        {
            Console.WriteLine("\n--- Kontrollera tillgänglighet av rum---");
            int roomId = GetIntInput("Ange Rums-ID: ");
            DateTime date = GetDateInput("Ange datum (yyyy-MM-dd): ");
            TimeSpan startTime = GetTimeInput("Ange starttid (HH:mm): ");
            TimeSpan endTime = GetTimeInput("Ange sluttid (HH:mm): ");

            var start = date.Add(startTime);
            var end = date.Add(endTime);


            string url = $"{ApiBaseUrl}/availability?roomId={roomId}&startTime={start:s}&endTime={end:s}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                bool isAvailable = await response.Content.ReadFromJsonAsync<bool>();
                if (isAvailable)
                {
                    Console.WriteLine("✅ Rummet är ledigt!");
                }
                else
                {
                    Console.WriteLine("⛔ Rummet är upptaget.");
                }
            }
            else
            {
                Console.WriteLine($"❌ Fel vid kontroll: {response.ReasonPhrase}");

            }
        }

        private static async Task ListBookingsAsync()
        {
            Console.WriteLine("\n--- Visa bokningar ---");
            int roomId = GetIntInput("Ange Rums-ID: ");

            var response = await _httpClient.GetAsync($"{ApiBaseUrl}?roomId={roomId}");

            if (response.IsSuccessStatusCode)
            {
                var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();

                if (bookings != null && bookings.Any())
                {
                    Console.WriteLine($"Hittade {bookings.Count} bokningar:");
                    foreach (var b in bookings)
                    {
                        Console.WriteLine($"- ID: {b.Id} | {b.StartTime} -> {b.EndTime} (User: {b.UserId})");
                    }
                }
                else
                {
                    Console.WriteLine("Inga bokningar hittades för detta rum.");
                }
            }
            else
            {
                Console.WriteLine($"❌ Kunde inte hämta bokningar: {response.ReasonPhrase}");
            }
        }

        private static async Task CreateBookingAsync()
        {
            Console.WriteLine("\n--- Skapa ny bokning ---");
            int roomId = GetIntInput("Ange rums-ID: ");
            int userId = GetIntInput("Ange användar-ID: ");

            DateTime date = GetDateInput("Ange bokningsdatum (YYYY-MM-DD): ");
            TimeSpan startTime = GetTimeInput("Ange starttid (HH:MM): ");
            TimeSpan endTime = GetTimeInput("Ange sluttid (HH:MM): ");

            var dto = new CreateBookingDto
            {
                RoomId = roomId,
                UserId = userId,
                StartTime = date.Add(startTime),
                EndTime = date.Add(endTime)
            };

            Console.WriteLine("Skickar bokningsförfrågan...");
            var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}", dto);

            if (response.IsSuccessStatusCode)
            {
                var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
                Console.WriteLine($"Bokning skapad! Boknings-ID: {createdBooking?.Id}");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Misslyckades: {errorMsg}");
            }

        }


        private static TimeSpan GetTimeInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (TimeSpan.TryParse(Console.ReadLine(), out TimeSpan result))
                {
                    return result;
                }
                Console.WriteLine("Ogiltigt tidsformat (ex: 14:30). Försök igen.");
            }
        }

        private static DateTime GetDateInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParse(Console.ReadLine(), out DateTime result))
                {
                    return result.Date;
                }
                Console.WriteLine("Ogiltigt datumformat (ex: 2023-12-24). Försök igen.");
            }
        }

        private static int GetIntInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result))
                {
                    return result;
                }
                Console.WriteLine("Ogiltigt tal. Försök igen.");
            }
        }
    }
}

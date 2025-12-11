using BookingSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Data
{
    public class BookingDbContext : DbContext
    {

        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {

        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<User> Users { get; set; }

    }
}

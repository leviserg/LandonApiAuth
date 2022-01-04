using LandonApiAuth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth
{
    public class HotelApiDbContext : IdentityDbContext<UserEntity, UserRoleEntity, Guid>
    {
        public HotelApiDbContext(DbContextOptions options) : base(options) {
            Database.EnsureCreated();
        }

        public DbSet<RoomEntity> Rooms { get; set; }

        public DbSet<BookingEntity> Bookings { get; set; }
    }
}

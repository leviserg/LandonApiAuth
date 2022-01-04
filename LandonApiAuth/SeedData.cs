using LandonApiAuth.Models;
using LandonApiAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth.Properties
{
    public static class SeedData
    {
        public static async Task InitializeIfEmptyAsync(IServiceProvider services)
        {
            await AddTestUsers(
                services.GetRequiredService<RoleManager<UserRoleEntity>>(),
                services.GetRequiredService<UserManager<UserEntity>>());

            await AddTestDataIfEmpty(
                services.GetRequiredService<HotelApiDbContext>(),
                services.GetRequiredService<IDateLogicService>());
        }

        public static async Task AddTestDataIfEmpty(
            HotelApiDbContext context,
            IDateLogicService dateLogicService)
        {
            
            if (context.Rooms.Any())
            {
                // Already has data
                return;
            }
            
            var pickford = context.Rooms.Add(new RoomEntity
            {
                Id = Guid.Parse("301df04d-8679-4b1b-ab92-0a586fe53d08"),
                Name = "Pickford Suite",
                Rate = 19999,
            }).Entity;

            context.Rooms.Add(new RoomEntity
            {
                Id = Guid.Parse("ee2b83be-91db-4de5-8122-35a9e9195976"),
                Name = "Driscoll Suite",
                Rate = 23959
            });

            var today = DateTimeOffset.Now;
            var start = dateLogicService.AlignStartTime(today);
            var end = start.Add(dateLogicService.GetMinimumStay());

            
            context.Bookings.Add(new BookingEntity
            {
                Id = Guid.Parse("2eac8dea-2749-42b3-9d21-8eb2fc0fd6bd"),
                Room = pickford,
                CreatedAt = DateTimeOffset.UtcNow,
                StartAt = start,
                EndAt = end,
                Total = pickford.Rate,
            });
            
            await context.SaveChangesAsync();
        }

        private static async Task AddTestUsers(
            RoleManager<UserRoleEntity> roleManager,
            UserManager<UserEntity> userManager)
        {
            var dataExists = roleManager.Roles.Any() || userManager.Users.Any();
            if (dataExists)
            {
                return;
            }

            // Add a test role
            await roleManager.CreateAsync(new UserRoleEntity("Admin"));

            // Add a test user
            var user = new UserEntity
            {
                Email = "admin@landon.local",
                UserName = "admin@landon.local",
                FirstName = "Admin",
                LastName = "Tester",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await userManager.CreateAsync(user, "Supersecret123!!");

            // Put the user in the admin role
            await userManager.AddToRoleAsync(user, "Admin");
            await userManager.UpdateAsync(user);
        }
    }
}

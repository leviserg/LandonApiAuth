using Microsoft.AspNetCore.Identity;
using System;

namespace LandonApiAuth.Models
{
    public class UserEntity : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}

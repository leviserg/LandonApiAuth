using LandonApiAuth.Models;
using System;

namespace LandonApiAuth.Models
{
    public class User : Resource
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}

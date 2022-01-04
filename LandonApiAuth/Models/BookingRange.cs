using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth.Models
{
    public class BookingRange
    {
        public DateTimeOffset StartAt { get; set; }

        public DateTimeOffset EndAt { get; set; }
    }
}

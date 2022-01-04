using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth.Models
{
    public class RoomsResponse : PagedCollection<Room>
    {
        public Link Openings { get; set; }

        public Form RoomsQuery { get; set; }
    }
}

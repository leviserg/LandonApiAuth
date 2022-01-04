using LandonApiAuth.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth.Models
{
    public class Room : Resource
    {
        [Sortable]
        [SearchableString]
        public string Name { get; set; }

        [Sortable(Default = true)]
        [SearchableDecimal]
        public decimal Rate { get; set; }

        public Form Book { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Models
{
    public class ApplicationUser
    {
        public string ConnectionId { get; set; }
        public Preference Self { get; set; }
        public Preference UserPreference { get; set; }
        public string GroupId { get; set; }
    }
}
